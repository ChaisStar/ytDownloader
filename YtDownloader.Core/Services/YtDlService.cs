using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace YtDownloader.Core.Services;

public class YtDlService(YtDlVideoOptionSet optionSet, YtDlVideoOptionSetMergeFlexible mergeFlexibleOptionSet, 
    YtDlVideoOptionSetNoThumbnail noThumbnailOptionSet, YtDlVideoOptionSetAutoMerge autoMergeOptionSet, 
    YtDlVideoOptionSetBestPreMerged bestPreMergedOptionSet, YtDlVideoOptionSetRawDownload rawDownloadOptionSet,
    YtDlVideoOptionSetVideoOnly videoOnlyOptionSet, YtDlMp3OptionSet mp3OptionSet) : IYtDlService
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
    
    private readonly OptionSet[] videoFallbacks = new[]
    {
        optionSet.Value,
        mergeFlexibleOptionSet.Value,
        noThumbnailOptionSet.Value,
        autoMergeOptionSet.Value,
        bestPreMergedOptionSet.Value,
        rawDownloadOptionSet.Value,
        videoOnlyOptionSet.Value
    };
    
    private readonly string[] fallbackNames = new[]
    {
        "primary",
        "flexible merge",
        "without thumbnail",
        "auto-merge",
        "best pre-merged",
        "raw download",
        "video-only"
    };

    public async Task<RunResult<string>> RunVideoDownload(string url, bool later = false, Action<DownloadProgress>? downloadProgressHandler = null)
    {
        RunResult<string>? lastResult = null;
        
        for (int i = 0; i < videoFallbacks.Length; i++)
        {
            var result = await youtubeDL.RunVideoDownload(url, overrideOptions: videoFallbacks[i], 
                progress: downloadProgressHandler is null ? null : new Progress<DownloadProgress>(downloadProgressHandler));
            
            lastResult = result;
            if (result.Success)
                return result;
                
            Console.WriteLine($"Trying {fallbackNames[i]} format...");
        }
        
        // Log final error if all attempts failed
        if (lastResult != null && !lastResult.Success)
        {
            Console.WriteLine($"All download formats failed. Final error: {string.Join("; ", lastResult.ErrorOutput ?? [])}");
        }
        
        return lastResult ?? new RunResult<string>(false, [], "");
    }

    public Task<RunResult<VideoData>> GetVideoData(string url) => youtubeDL.RunVideoDataFetch(url, overrideOptions: optionSet.Value);

    public Task<RunResult<string>> RunMp3PlaylistDownload(string url) => mp3Dl.RunVideoDownload(url, overrideOptions: mp3OptionSet.Value);

    public string GetVersion () => youtubeDL.Version;

    public Task<string> Update () => youtubeDL.RunUpdate();
}