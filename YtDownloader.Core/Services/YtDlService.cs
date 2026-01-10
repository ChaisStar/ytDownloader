using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;
using System.Diagnostics;

namespace YtDownloader.Core.Services;

public class YtDlService(YtDlMainOptionSet mainOptionSet, YtDlVideoOptionSet optionSet, YtDlVideoOptionSetMergeFlexible mergeFlexibleOptionSet, 
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
        mainOptionSet.Value,
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
        "main",
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

    public Task<RunResult<VideoData>> GetVideoData(string url) => youtubeDL.RunVideoDataFetch(url, overrideOptions: mainOptionSet.Value);

    public Task<RunResult<string>> RunMp3PlaylistDownload(string url) => mp3Dl.RunVideoDownload(url, overrideOptions: mp3OptionSet.Value);

    public async Task<string> GetVersion()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh",
                    Arguments = OperatingSystem.IsWindows() ? "/c yt-dlp --version" : "-c \"yt-dlp --version\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            
            return output.Trim() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    public Task<string> Update() => youtubeDL.RunUpdate();
}