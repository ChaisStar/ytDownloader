using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace YtDownloader.Core.Services;

public class YtDlService(YtDlVideoOptionSet optionSet, YtDlVideoOptionSetMergeFlexible mergeFlexibleOptionSet, 
    YtDlVideoOptionSetNoThumbnail noThumbnailOptionSet, YtDlVideoOptionSetAutoMerge autoMergeOptionSet, 
    YtDlVideoOptionSetBestPreMerged bestPreMergedOptionSet, YtDlVideoOptionSetRawDownload rawDownloadOptionSet, YtDlMp3OptionSet mp3OptionSet) : IYtDlService
{
    private const string outputFolder = "/tmp";
    private static readonly string youtubeDLPath = OperatingSystem.IsLinux() ? "yt-dlp" : @"C:\Users\Chais Star\.stacher\yt-dlp.exe";
    private static readonly string fFmpegPath = OperatingSystem.IsLinux() ? "ffmpeg" : @"C:\Users\Chais Star\.stacher\ffmpeg.exe";
    
    private readonly YoutubeDL youtubeDL = new ()
    {
        YoutubeDLPath = youtubeDLPath,
        FFmpegPath = fFmpegPath,
        OutputFolder = outputFolder,
        OutputFileTemplate = "%(upload_date)s_%(title)s.%(ext)s",
        OverwriteFiles = true
    };

    private readonly YoutubeDL mp3Dl = new ()
    {
        YoutubeDLPath = youtubeDLPath,
        FFmpegPath = fFmpegPath,
        OutputFolder = outputFolder,
        OutputFileTemplate = "%(title)s.%(ext)s",
        OverwriteFiles = true
    };

    public async Task<RunResult<string>> RunVideoDownload(string url, bool later = false, Action<DownloadProgress>? downloadProgressHandler = null)
    {
        var optionSets = new[]
        {
            (optionSet.Value, "primary"),
            (mergeFlexibleOptionSet.Value, "flexible merge"),
            (noThumbnailOptionSet.Value, "without thumbnail"),
            (autoMergeOptionSet.Value, "auto-merge"),
            (bestPreMergedOptionSet.Value, "best pre-merged"),
            (rawDownloadOptionSet.Value, "raw download")
        };

        RunResult<string> result = new() { Success = false };
        
        foreach (var (options, name) in optionSets)
        {
            result = await youtubeDL.RunVideoDownload(url, overrideOptions: options, 
                progress: downloadProgressHandler is null ? null : new Progress<DownloadProgress>(downloadProgressHandler));
            
            if (result.Success)
                break;
                
            Console.WriteLine($"Trying {name} format...");
        }
        
        return result;
    }

    public Task<RunResult<VideoData>> GetVideoData(string url) => youtubeDL.RunVideoDataFetch(url, overrideOptions: optionSet.Value);

    public Task<RunResult<string>> RunMp3PlaylistDownload(string url) => mp3Dl.RunVideoDownload(url, overrideOptions: mp3OptionSet.Value);

    public string GetVersion () => youtubeDL.Version;

    public Task<string> Update () => youtubeDL.RunUpdate();
}