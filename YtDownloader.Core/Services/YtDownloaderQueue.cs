using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using YtDownloader.Base.Models;

namespace YtDownloader.Core.Services;

public class YtDownloaderQueue(IServiceScopeFactory scopeFactory, ILogger<YtDownloaderQueue> logger) : BackgroundService
{
    private const int MaxSimultanouslyDownloads = 5;
    private const int CheckTimeout = 5 * 1000;

    private readonly SemaphoreSlim _semaphoreSlim = new(MaxSimultanouslyDownloads, MaxSimultanouslyDownloads);
    private readonly HashSet<int> _activeDownloads = [];
    private readonly object _lock = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("YtDownloaderQueue starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var mainScope = scopeFactory.CreateScope();
                var downloadService = mainScope.ServiceProvider.GetRequiredService<IDownloadService>();

                var undefinedDownloads = await downloadService.GetUndefinedDownloads();
                foreach (var download in undefinedDownloads)
                {
                    lock (_lock)
                    {
                        if (!_activeDownloads.Add(download.Id)) continue;
                    }
                    
                    _ = Task.Run(async () =>
                    {
                        try { await UpdateInfoAsync(download); }
                        finally { lock (_lock) { _activeDownloads.Remove(download.Id); } }
                    }, stoppingToken);
                }

                var queuedDownloads = await downloadService.GetPendingDownloads();
                var failedDownloads = await downloadService.GetFailedDownloads();
                
                // Combine and prioritize
                var failedIds = failedDownloads.Select(d => d.Id).ToHashSet();
                var sortedDownloads = queuedDownloads.Concat(failedDownloads)
                    .OrderBy(d => failedIds.Contains(d.Id) ? 1 : 0)
                    .ThenBy(d => d.Later)
                    .ThenByDescending(d => d.Created)
                    .ToList();

                foreach (var download in sortedDownloads)
                {
                    lock (_lock)
                    {
                        if (!_activeDownloads.Add(download.Id)) continue;
                    }

                    logger.LogInformation("Enqueuing download {DownloadId}", download.Id);
                    
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await UpdateInfoAsync(download);
                            await RunAsync(download);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Background task error for {DownloadId}", download.Id);
                        }
                        finally
                        {
                            lock (_lock) { _activeDownloads.Remove(download.Id); }
                        }
                    }, stoppingToken);
                }

                await Task.Delay(CheckTimeout, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in YtDownloaderQueue loop");
                await Task.Delay(CheckTimeout, stoppingToken);
            }
        }
    }

    private async Task UpdateInfoAsync(Download download)
    {
        using var scope = scopeFactory.CreateScope();
        var downloadService = scope.ServiceProvider.GetRequiredService<IDownloadService>();

        try
        {
            logger.LogInformation("Starting UpdateInfo for download {DownloadId}", download.Id);
            await downloadService.UpdateInfo(download);
            logger.LogInformation("Successfully updated info for download {DownloadId}: Title={Title}, Size={Size}", download.Id, download.Title, download.TotalSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update download info for {DownloadId}", download.Id);
            // Mark as failed with error message
            var errorMessage = $"Failed to fetch video info: {ex.Message}";
            var failedDownload = download;
            failedDownload.Fail(errorMessage);
            await downloadService.Fail(failedDownload);
        }
    }

    private async Task RunAsync(Download download)
    {
        using var scope = scopeFactory.CreateScope();
        var downloadService = scope.ServiceProvider.GetRequiredService<IDownloadService>();

        try
        {
            await _semaphoreSlim.WaitAsync();
            logger.LogInformation("Starting download {DownloadId}: {Url}", download.Id, download.Url);
            
            await downloadService.Start(download);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Download error: {ex.Message}";
            download.Fail(errorMessage);
            await downloadService.Fail(download);
            logger.LogError(ex, "Failed to start download {DownloadId}", download.Id);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public override void Dispose()
    {
        _semaphoreSlim.Dispose();
        base.Dispose();
    }
}