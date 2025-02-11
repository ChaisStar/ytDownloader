using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace YtDownloader.Core.Services;

internal interface IYtDlService
{
    Task<RunResult<string>> RunVideoDownloadAsync(string url, bool later = false, Action<DownloadProgress>? downloadProgressHandler = null);

    Task<RunResult<VideoData>> GetVideoDataAsync(string url);
}