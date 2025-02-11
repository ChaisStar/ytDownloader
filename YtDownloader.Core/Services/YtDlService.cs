using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace YtDownloader.Core.Services;

public class YtDlService(YtDlOptionSet optionSet) : IYtDlService
{
    private readonly YoutubeDL youtubeDL = new ()
    {
        YoutubeDLPath = OperatingSystem.IsLinux() ? "yt-dlp" : @"C:\Users\Chais Star\.stacher\yt-dlp.exe",
        FFmpegPath = OperatingSystem.IsLinux() ? "ffmpeg" : @"C:\Users\Chais Star\.stacher\ffmpeg.exe",
        OutputFolder = "/tmp",
        OutputFileTemplate = "%(upload_date)s_%(title)s.%(ext)s",
        OverwriteFiles = true
    };

    public Task<RunResult<string>> RunVideoDownloadAsync(string url, bool later = false, Action<DownloadProgress>? downloadProgressHandler = null) => 
        youtubeDL.RunVideoDownload(url, overrideOptions: optionSet.Value, progress: downloadProgressHandler is null ? null : new Progress<DownloadProgress>(downloadProgressHandler));

    public Task<RunResult<VideoData>> GetVideoDataAsync(string url) => youtubeDL.RunVideoDataFetch(url, overrideOptions: optionSet.Value);
}