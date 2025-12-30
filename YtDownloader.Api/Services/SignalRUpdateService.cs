using YtDownloader.Api.Models;
using YtDownloader.Base.Enums;
using YtDownloader.Base.Repositories;
using System.IO;

namespace YtDownloader.Api.Services;

public class SignalRUpdateService(ISignalRBroadcaster broadcaster, IDownloadRepository downloadRepository) : BackgroundService
{
    private readonly PeriodicTimer _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(200));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    // Broadcast current downloads (excluding old finished ones)
                    var downloads = await downloadRepository.Get(
                        DownloadStatus.Pending,
                        DownloadStatus.Downloading,
                        DownloadStatus.Finished
                    );

                    // Filter out finished downloads older than 5 days
                    var fiveDaysAgo = DateTime.UtcNow.AddDays(-5);
                    var currentDownloads = downloads
                        .Where(d => d.Status != DownloadStatus.Finished || d.Finished >= fiveDaysAgo)
                        .Select(d => new DownloadResponse(d))
                        .ToList();

                    await broadcaster.BroadcastDownloadsUpdate(currentDownloads);

                    // Broadcast yt-dlp version
                    var versionFile = "/etc/yt-dlp-version.txt";
                    if (File.Exists(versionFile))
                    {
                        var version = await File.ReadAllTextAsync(versionFile, stoppingToken);
                        await broadcaster.BroadcastVersionUpdate(version.Trim());
                    }

                    // Broadcast cookies info
                    var cookiesPath = "/tmp/cookies/cookies.txt";
                    if (File.Exists(cookiesPath))
                    {
                        var fileInfo = new FileInfo(cookiesPath);
                        var cookiesInfo = new CookiesInfoDto
                        {
                            LastModified = fileInfo.LastWriteTimeUtc,
                            Size = fileInfo.Length,
                            Exists = true
                        };
                        await broadcaster.BroadcastCookiesUpdate(cookiesInfo);
                    }
                    else
                    {
                        await broadcaster.BroadcastCookiesUpdate(new CookiesInfoDto { Exists = false, Size = 0 });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in SignalR update broadcast: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when service is stopping
        }
        finally
        {
            _timer.Dispose();
        }
    }
}
