using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using YtDownloader.Base.Models;

namespace YtDownloader.Core.Services;

public class YtDownloaderQueue(IDownloadService downloadService, ILogger<YtDownloaderQueue> logger) : BackgroundService
{
    private const int MaxSimultanouslyDownloads = 5;
    private const int CheckTimeout = 5 * 1000;

    private readonly SemaphoreSlim _semaphoreSlim = new(MaxSimultanouslyDownloads, MaxSimultanouslyDownloads);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("YtDownloaderQueue starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var undefinedDownloads = await downloadService.GetUndefinedDownloads();
                logger.LogInformation("Found {UndefinedCount} undefined downloads", undefinedDownloads.Count);
                foreach (var download in undefinedDownloads)
                {
                    logger.LogInformation("Updating info for download {DownloadId}: {Url}", download.Id, download.Url);
                    await UpdateInfoAsync(download);
                }

                var queuedDownloads = await downloadService.GetPendingDownloads();
                var failedDownloads = await downloadService.GetFailedDownloads();
                logger.LogInformation("Found {PendingCount} pending and {FailedCount} failed downloads", queuedDownloads.Count, failedDownloads.Count);
                
                // Create a set of failed IDs for efficient lookup
                var failedIds = failedDownloads.Select(d => d.Id).ToHashSet();
                
                // Combine all downloads and sort: pending first, then failed
                var allDownloads = new List<Download>();
                allDownloads.AddRange(queuedDownloads);
                allDownloads.AddRange(failedDownloads);
                
                // Sort by: pending status first, then non-"later", then by creation date
                var sortedDownloads = allDownloads
                    .OrderBy(d => failedIds.Contains(d.Id) ? 1 : 0)  // Pending (0) before Failed (1)
                    .ThenBy(d => d.Later)  // Non-"later" before "later"
                    .ThenByDescending(d => d.Created)  // Newest first
                    .ToList();

                foreach (var download in sortedDownloads)
                {
                    logger.LogInformation("Processing download {DownloadId}: {Url}", download.Id, download.Url);
                    _ = Task.Run(async () =>
                    {
                        await UpdateInfoAsync(download);
                        await RunAsync(download);
                    });
                }

                await Task.Delay(CheckTimeout, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in YtDownloaderQueue loop");
                await Task.Delay(CheckTimeout, stoppingToken);
            }
        }

        logger.LogInformation("YtDownloaderQueue stopping.");
    }

    private async Task UpdateInfoAsync(Download download)
    {
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