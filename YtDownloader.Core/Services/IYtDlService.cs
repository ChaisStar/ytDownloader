using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace YtDownloader.Core.Services;

internal interface IYtDlService
{
    Task<RunResult<string>> RunVideoDownload(string url, bool later = false, Action<DownloadProgress>? downloadProgressHandler = null);

    Task<RunResult<VideoData>> GetVideoData(string url);

    Task<RunResult<string>> RunMp3PlaylistDownload(string url);
}