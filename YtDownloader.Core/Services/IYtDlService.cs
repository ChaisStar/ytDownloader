using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace YtDownloader.Core.Services;

public interface IYtDlService
{
    Task<RunResult<string>> RunVideoDownload(string url, Action<DownloadProgress>? downloadProgressHandler = null);

    Task<RunResult<VideoData>> GetVideoData(string url);

    Task<RunResult<string>> RunMp3PlaylistDownload(string url);

    Task<string> GetVersion();
}