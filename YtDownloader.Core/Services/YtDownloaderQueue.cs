using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using YtDownloader.Base.Models;

namespace YtDownloader.Core.Services;

public class YtDownloaderQueue(IServiceProvider serviceProvider, ILogger<YtDownloaderQueue> logger) : BackgroundService
{
    private const int MaxSimultanouslyDownloads = 5;
    private const int CheckTimeout = 5 * 1000;

    private readonly HashSet<int> _queueIds = [];
    private readonly SemaphoreSlim _semaphoreSlim = new(MaxSimultanouslyDownloads, MaxSimultanouslyDownloads);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("YtDownloaderQueue starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var undefinedDownloads = await DownloadService.GetUndefinedDownloads();
            foreach (var download in undefinedDownloads)
            {
                await UpdateInfoAsync(download);
            }

            var queuedDownloads = await DownloadService.GetPendingDownloads();
            var newPending = queuedDownloads.Where(qi => !_queueIds.Contains(qi.Id)).ToList();
            
            var failedDownloads = await DownloadService.GetFailedDownloads();
            var newFailed = failedDownloads.Where(fd => !_queueIds.Contains(fd.Id)).ToList();
            
            // Create a set of failed IDs for efficient lookup
            var failedIds = newFailed.Select(d => d.Id).ToHashSet();
            
            // Combine all new downloads and sort: pending first, then failed
            var allDownloads = new List<Download>();
            allDownloads.AddRange(newPending);
            allDownloads.AddRange(newFailed);
            
            // Sort by: pending status first, then non-"later", then by creation date
            var sortedDownloads = allDownloads
                .OrderBy(d => failedIds.Contains(d.Id) ? 1 : 0)  // Pending (0) before Failed (1)
                .ThenBy(d => d.Later)  // Non-"later" before "later"
                .ThenByDescending(d => d.Created)  // Newest first
                .ToList();

            foreach (var download in sortedDownloads)
            {
                await UpdateInfoAsync(download);
                _queueIds.Add(download.Id);
                _ = RunAsync(download);
            }

            await Task.Delay(CheckTimeout, stoppingToken);
        }

        logger.LogInformation("YtDownloaderQueue stopping.");
    }

    private async Task UpdateInfoAsync(Download download)
    {
        try
        {
            await DownloadService.UpdateInfo(download);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update download info for {DownloadId}", download.Id);
            // Mark as failed with error message
            var errorMessage = $"Failed to fetch video info: {ex.Message}";
            var failedDownload = download;
            failedDownload.Fail(errorMessage);
            using var scope = serviceProvider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IDownloadService>();
            await repo.Fail(failedDownload);
        }
    }

    private async Task RunAsync(Download download)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            await DownloadService.Start(download);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Download error: {ex.Message}";
            download.Fail(errorMessage);
            await DownloadService.Fail(download);
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

    private IDownloadService DownloadService => serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IDownloadService>();
}