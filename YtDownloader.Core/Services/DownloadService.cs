using YoutubeDLSharp;

using YtDownloader.Base.Enums;
using YtDownloader.Base.Models;
using YtDownloader.Base.Repositories;
using YtDownloader.Core.Extestions;

namespace YtDownloader.Core.Services;

internal class DownloadService(IDownloadRepository repository, IYtDlService ytDlService) : IDownloadService
{
    private const string None = "none";

    public Task<IReadOnlyList<Download>> GetPendingDownloads() => repository.Get(DownloadStatus.Pending);

    public Task<IReadOnlyList<Download>> GetFailedDownloads() => repository.Get(DownloadStatus.Failed);

    public Task<IReadOnlyList<Download>> GetUndefinedDownloads() => repository.GetUndefined();

    public async Task UpdateInfo(Download download)
    {
        var metadata = await ytDlService.GetVideoData(download.Url);

        // Best video (720p to 1080p, 30 FPS, ~5Mbps max)
        var bestVideo = metadata.Data.Formats?
            .Where(f => f.VideoCodec != "none" && f.VideoCodec != null &&
                        (f.Height ?? 0) <= 1080 && (f.Height ?? 0) >= 720 &&
                        (f.FrameRate ?? 0) <= 30 && (f.VideoBitrate ?? 0) <= 5000)
            .OrderByDescending(f => f.Height ?? 0)
            .ThenByDescending(f => f.VideoBitrate ?? 0)
            .FirstOrDefault();

        // Best audio (m4a up to 128kbps, fallback to any m4a)
        var bestAudio = metadata.Data.Formats?
            .Where(f => f.AudioCodec != "none" && f.AudioCodec != null &&
                        f.VideoCodec == "none" && f.Extension == "m4a" &&
                        (f.AudioBitrate ?? 0) <= 128)
            .OrderByDescending(f => f.AudioBitrate ?? 0)
            .FirstOrDefault() ?? metadata.Data.Formats?
            .Where(f => f.AudioCodec != "none" && f.AudioCodec != null &&
                        f.VideoCodec == "none" && f.Extension == "m4a")
            .OrderByDescending(f => f.AudioBitrate ?? 0)
            .FirstOrDefault();

        // Debugging info
        Console.WriteLine($"Best Video: Codec={bestVideo?.VideoCodec}, Height={bestVideo?.Height}, FPS={bestVideo?.FrameRate}, Bitrate={bestVideo?.VideoBitrate}");
        Console.WriteLine($"Best Audio: Codec={bestAudio?.AudioCodec}, Format={bestAudio?.Extension}, Bitrate={bestAudio?.AudioBitrate}");

        // Calculate sizes
        long videoSize = bestVideo?.FileSize ?? bestVideo?.ApproximateFileSize ?? 0;
        long audioSize = bestAudio?.FileSize ?? bestAudio?.ApproximateFileSize ?? 0;
        long totalSize = videoSize + audioSize;

        var columnsToUpdate = download.SetInfo(metadata.Data.Title, totalSize, metadata.Data.Thumbnail);
        await repository.Update(download, columnsToUpdate);
    }

    public async Task Start(Download item)
    {
        var columnsToUpdate = item.Start();
        await repository.Update(item, columnsToUpdate);
        var result = await ytDlService.RunVideoDownload(item.Url, item.Later, p => HandleProgress(item, p));
        var fileSize = new FileInfo(result.Data).Length;
        columnsToUpdate = item.Finish(fileSize);
        await repository.Update(item, columnsToUpdate);

        var filename = result.Data.SanitizeFileName();
        File.Move(result.Data, $"/tmp/youtube{(item.Later ? "_later" : "")}/{filename}");
        File.Create($"/tmp/finished/{filename}.done");
    }

    private void HandleProgress(Download item, DownloadProgress progress)
    {
        try
        {
            if (progress.State != DownloadState.Downloading)
            {
                return;
            }
            var newProgress = Convert.ToInt32(progress.Progress * 100);
            if (newProgress > item.Progress)
            {
                var columnsToUpdate = item.UpdateProgress(newProgress, progress.DownloadSpeed, progress.ETA);
                repository.Update(item, columnsToUpdate).GetAwaiter().GetResult();
            }
        }
        catch
        {
        }
    }

    public Task Fail(Download item)
    {
        var columnsToUpdate = item.Fail();
        return repository.Update(item, columnsToUpdate);
    }
}