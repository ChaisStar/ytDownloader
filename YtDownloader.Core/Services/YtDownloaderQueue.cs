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

            var failedDownloads = await DownloadService.GetFailedDownloads();
            foreach (var download in failedDownloads)
            {
                await UpdateInfoAsync(download);
                _queueIds.Add(download.Id);
                _ = RunAsync(download);
            }

            var queuedDownloads = await DownloadService.GetPendingDownloads();
            var newDownloads = queuedDownloads.Where(qi => !_queueIds.Contains(qi.Id)).ToList();

            foreach (var download in newDownloads)
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
        catch (Exception)
        {
            logger.LogError("Failed to update download info: {id}.", download.Id);
        }
    }

    private async Task RunAsync(Download download)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            await DownloadService.Start(download);
        }
        catch(Exception)
        {
            await DownloadService.Fail(download);
            logger.LogError("Failed to start download: {id}.", download.Id);
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