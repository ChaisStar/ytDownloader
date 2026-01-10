using YoutubeDLSharp;

using YtDownloader.Base.Enums;
using YtDownloader.Base.Models;
using YtDownloader.Base.Repositories;
using YtDownloader.Core.Extestions;

namespace YtDownloader.Core.Services;

internal class DownloadService(IDownloadRepository repository, IYtDlService ytDlService) : IDownloadService
{
    public Task<IReadOnlyList<Download>> GetPendingDownloads() => repository.Get(DownloadStatus.Pending);

    public Task<IReadOnlyList<Download>> GetFailedDownloads() => repository.Get(DownloadStatus.Failed);

    public Task<IReadOnlyList<Download>> GetUndefinedDownloads() => repository.GetUndefined();

    public async Task UpdateInfo(Download download)
    {
        var metadata = await ytDlService.GetVideoData(download.Url);
        
        if (!metadata.Success)
        {
            throw new InvalidOperationException($"Failed to fetch video metadata: {string.Join(Environment.NewLine, metadata.ErrorOutput)}");
        }

        // Best video (720p to 1080p, 30 FPS)
        var bestVideo = metadata.Data.Formats?
            .Where(f => f.VideoCodec != "none" && f.VideoCodec != null &&
                        (f.Height ?? 0) <= 1080 && (f.Height ?? 0) >= 720 &&
                        (f.FrameRate ?? 0) <= 30)
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

    public async Task<Download> Start(string url, int? tagId = null)
    {
        var item = await repository.Create(url, tagId);
        return item;
    }

    public async Task Start(Download item)
    {
        var columnsToUpdate = item.Start();
        await repository.Update(item, columnsToUpdate);
        
        var result = await ytDlService.RunVideoDownload(item.Url, p => HandleProgress(item, p));
        
        if (!result.Success)
        {
            var errorMessage = $"Download failed: {string.Join(Environment.NewLine, result.ErrorOutput)}";
            var failColumns = item.Fail(errorMessage);
            await repository.Update(item, failColumns);
            return;
        }
        
        var fileSize = new FileInfo(result.Data).Length;
        columnsToUpdate = item.Finish(fileSize);
        await repository.Update(item, columnsToUpdate);

        var youtubeDirectory = "/tmp/youtube";
        var filename = result.Data.SanitizeFileName();

        if (item.Tag != null)
        {
            switch (item.Tag.Usage)
            {
                case TagUsage.Directory:
                    youtubeDirectory = $"/tmp/{item.Tag.Value}";
                    break;
                case TagUsage.Prefix:
                    filename = $"{item.Tag.Value}_{filename}";
                    break;
                case TagUsage.Suffix:
                    var ext = Path.GetExtension(filename);
                    var nameOnly = Path.GetFileNameWithoutExtension(filename);
                    filename = $"{nameOnly}_{item.Tag.Value}{ext}";
                    break;
            }
        }

        Directory.CreateDirectory(youtubeDirectory);
        var finalFilename = filename.GetUniqueFileName(youtubeDirectory);
        var finalPath = Path.Combine(youtubeDirectory, finalFilename);
        
        File.Move(result.Data, finalPath, overwrite: false);
        
        // Create marker file with the same naming strategy
        var finishedDirectory = "/tmp/finished";
        Directory.CreateDirectory(finishedDirectory);
        var doneFileName = $"{finalFilename}.done";
        var donePath = doneFileName.GetUniqueFileName(finishedDirectory);
        var finalDonePath = Path.Combine(finishedDirectory, donePath);
        
        using (var doneFile = File.Create(finalDonePath))
        {
            // File created successfully
        }
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
                // Calculate speed and ETA from available progress data
                string speed = "0 B/s";
                string eta = "--:--:--";

                if (item.TotalSize.HasValue && item.TotalSize > 0 && item.Started.HasValue)
                {
                    var elapsedSeconds = (DateTime.UtcNow - item.Started.Value).TotalSeconds;
                    if (elapsedSeconds > 0)
                    {
                        // Calculate bytes downloaded based on progress percentage
                        long bytesDownloaded = (long)(item.TotalSize.Value * (newProgress / 100.0));
                        double downloadSpeedBytesPerSec = bytesDownloaded / elapsedSeconds;
                        
                        // Format speed
                        speed = FormatBytes(downloadSpeedBytesPerSec) + "/s";
                        
                        // Calculate ETA
                        if (downloadSpeedBytesPerSec > 0 && newProgress < 100)
                        {
                            long remainingBytes = item.TotalSize.Value - bytesDownloaded;
                            double remainingSeconds = remainingBytes / downloadSpeedBytesPerSec;
                            
                            int hours = (int)(remainingSeconds / 3600);
                            int minutes = (int)((remainingSeconds % 3600) / 60);
                            int seconds = (int)(remainingSeconds % 60);
                            
                            eta = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
                        }
                    }
                }

                var columnsToUpdate = item.UpdateProgress(newProgress, speed, eta);
                repository.Update(item, columnsToUpdate).GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            // Progress updates are non-critical, log but don't fail the download
            Console.WriteLine($"Error updating progress for download {item.Id}: {ex.Message}");
        }
    }

    private static string FormatBytes(double bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.00} {sizes[order]}";
    }

    public Task Fail(Download download)
    {
        var columnsToUpdate = download.Fail();
        return repository.Update(download, columnsToUpdate);
    }
}