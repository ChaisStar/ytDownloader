using YoutubeDLSharp;

using YtDownloader.Base.Enums;
using YtDownloader.Base.Models;
using YtDownloader.Base.Repositories;

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

        // Best video (720p to 1080p, ~5Mbps max)
        var bestVideo = metadata.Data.Formats?
            .Where(f => f.VideoCodec != None && f.VideoCodec != null &&
                        (f.Height ?? 0) <= 1080 && (f.Height ?? 0) >= 720 &&
                        (f.VideoBitrate ?? 0) <= 5000 &&
                        (f.FileSize is not null || f.ApproximateFileSize is not null))
            .OrderByDescending(f => f.Height ?? 0)
            .ThenByDescending(f => f.VideoBitrate ?? 0)
            .FirstOrDefault();

        // Best audio (prefer m4a up to 128kbps, fallback to any audio)
        var bestAudio = 
            metadata.Data.Formats?
                .Where(f => f.AudioCodec != None && f.AudioCodec != null &&
                            f.VideoCodec == None && f.Extension == "m4a" &&
                            (f.AudioBitrate ?? 0) <= 128 &&
                            (f.FileSize is not null || f.ApproximateFileSize is not null))
                .OrderByDescending(f => f.AudioBitrate ?? 0)
                .FirstOrDefault() 
            ?? metadata.Data.Formats?
                .Where(f => f.AudioCodec != None && f.AudioCodec != null &&
                            f.VideoCodec == None &&
                            (f.FileSize is not null || f.ApproximateFileSize is not null))
                .OrderByDescending(f => f.AudioBitrate ?? 0)
                .FirstOrDefault();

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

        var filename = result.Data.Replace("/tmp/", "");
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