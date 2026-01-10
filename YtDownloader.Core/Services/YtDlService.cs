using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using YtDownloader.Base.Repositories;
using YtDownloader.Base.Models;

namespace YtDownloader.Core.Services;

public class YtDlService(IServiceScopeFactory scopeFactory) : IYtDlService
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

    public async Task<RunResult<string>> RunVideoDownload(string url, Action<DownloadProgress>? downloadProgressHandler = null)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOptionSetRepository>();
        var optionSets = await repository.GetEnabled();

        RunResult<string>? lastResult = null;
        
        foreach (var optionSetModel in optionSets.OrderBy(o => o.Priority))
        {
            try
            {
                var options = optionSetModel.ToYtDlOptions();
                var result = await youtubeDL.RunVideoDownload(url, overrideOptions: options, 
                    progress: downloadProgressHandler is null ? null : new Progress<DownloadProgress>(downloadProgressHandler));
                
                lastResult = result;
                if (result.Success)
                    return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during {optionSetModel.Name} attempt: {ex.Message}");
                lastResult = new RunResult<string>(false, [ex.Message], "");
            }
                
            Console.WriteLine($"Trying {optionSetModel.Name} format...");
        }
        
        // Log final error if all attempts failed
        if (lastResult != null && !lastResult.Success)
        {
            Console.WriteLine($"All download formats failed. Final error: {string.Join("; ", lastResult.ErrorOutput ?? [])}");
        }
        
        return lastResult ?? new RunResult<string>(false, [], "");
    }

    public async Task<RunResult<VideoData>> GetVideoData(string url)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOptionSetRepository>();
        var optionSets = await repository.GetEnabled();
        
        // Use first enabled option set as default for metadata fetching (usually 'main')
        var defaultOptions = optionSets.FirstOrDefault()?.ToYtDlOptions() ?? new OptionSet { Cookies = "/tmp/cookies/cookies.txt" };
        
        return await youtubeDL.RunVideoDataFetch(url, overrideOptions: defaultOptions);
    }

    public async Task<RunResult<string>> RunMp3PlaylistDownload(string url)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOptionSetRepository>();
        var allOptions = await repository.GetAll();
        
        // Find mp3 specific options or fallback to a default
        var mp3Options = allOptions.FirstOrDefault(o => o.Name.Contains("mp3", StringComparison.OrdinalIgnoreCase) && o.IsEnabled);

        var options = mp3Options?.ToYtDlOptions() ?? new OptionSet 
        { 
            ExtractAudio = true, 
            AudioFormat = AudioConversionFormat.Mp3, 
            Cookies = "/tmp/cookies/cookies.txt" 
        };
        
        return await mp3Dl.RunVideoDownload(url, overrideOptions: options);
    }

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