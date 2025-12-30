using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace YtDownloader.Core.Services;

public class YtDlService(YtDlVideoOptionSet optionSet, YtDlVideoOptionSetMergeFlexible mergeFlexibleOptionSet, 
    YtDlVideoOptionSetNoThumbnail noThumbnailOptionSet, YtDlVideoOptionSetAutoMerge autoMergeOptionSet, YtDlMp3OptionSet mp3OptionSet) : IYtDlService
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
        // Try primary format
        var result = await youtubeDL.RunVideoDownload(url, overrideOptions: optionSet.Value, 
            progress: downloadProgressHandler is null ? null : new Progress<DownloadProgress>(downloadProgressHandler));
        
        // If primary format failed, try merge flexible format
        if (!result.Success)
        {
            Console.WriteLine($"Primary format failed, error output: {string.Join("; ", result.ErrorOutput ?? [])}");
            Console.WriteLine("Trying flexible merge format...");
            result = await youtubeDL.RunVideoDownload(url, overrideOptions: mergeFlexibleOptionSet.Value, 
                progress: downloadProgressHandler is null ? null : new Progress<DownloadProgress>(downloadProgressHandler));
        }
        
        // If still failing, try without thumbnail
        if (!result.Success)
        {
            Console.WriteLine($"Flexible merge format failed, error output: {string.Join("; ", result.ErrorOutput ?? [])}");
            Console.WriteLine("Trying without thumbnail...");
            result = await youtubeDL.RunVideoDownload(url, overrideOptions: noThumbnailOptionSet.Value, 
                progress: downloadProgressHandler is null ? null : new Progress<DownloadProgress>(downloadProgressHandler));
        }
        
        // If still failing, try auto-merge (no format restriction)
        if (!result.Success)
        {
            Console.WriteLine($"No-thumbnail format failed, error output: {string.Join("; ", result.ErrorOutput ?? [])}");
            Console.WriteLine("Trying auto-merge format with no restrictions...");
            result = await youtubeDL.RunVideoDownload(url, overrideOptions: autoMergeOptionSet.Value, 
                progress: downloadProgressHandler is null ? null : new Progress<DownloadProgress>(downloadProgressHandler));
        }
        
        return result;
    }

    public Task<RunResult<VideoData>> GetVideoData(string url) => youtubeDL.RunVideoDataFetch(url, overrideOptions: optionSet.Value);

    public Task<RunResult<string>> RunMp3PlaylistDownload(string url) => mp3Dl.RunVideoDownload(url, overrideOptions: mp3OptionSet.Value);

    public string GetVersion () => youtubeDL.Version;

    public Task<string> Update () => youtubeDL.RunUpdate();
}