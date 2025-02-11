using YoutubeDLSharp;

using YtDownloader.Base.Enums;
using YtDownloader.Base.Models;
using YtDownloader.Base.Repositories;

namespace YtDownloader.Core.Services;

internal class DownloadService(IDownloadRepository repository, IYtDlService ytDlService) : IDownloadService
{
    public Task<IReadOnlyList<Download>> GetPendingDownloads() => repository.Get(DownloadStatus.Pending);

    public async Task UpdateInfo(Download download)
    {
        var metadata = await ytDlService.GetVideoDataAsync(download.Url);

        // Best video
        var bestVideo = metadata.Data.Formats?
            .Where(f => f.VideoCodec != "none") // Only video formats
            .OrderByDescending(f => f.Height) // Best quality
            .FirstOrDefault();

        // Best audio in m4a format
        var bestAudio = metadata.Data.Formats?
            .Where(f => f.VideoCodec != "none" && f.Extension == "m4a") // Only m4a audio
            .OrderByDescending(f => f.AudioBitrate) // Best audio quality
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
        var result = await ytDlService.RunVideoDownloadAsync(item.Url, item.Later, p => HandleProgress(item, p));
        columnsToUpdate = item.Finish();
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