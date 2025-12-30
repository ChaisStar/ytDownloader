using YoutubeDLSharp.Options;

namespace YtDownloader.Core.Services;

// Primary format: Download best available single file
public class YtDlVideoOptionSet : Lazy<OptionSet>
{
    public YtDlVideoOptionSet() : base(new OptionSet
    {
        MergeOutputFormat = DownloadMergeFormat.Mp4,
        Format = "bestvideo[height<=1080][height>=720][fps<=30]+bestaudio[ext=m4a][abr<=128]/bestvideo[height<=1080][fps<=30]+bestaudio[ext=m4a]/best[height<=1080]",
        EmbedThumbnail = true,
        Cookies = "/tmp/cookies/cookies.txt",
    })
    { }
}

// Fallback 1: Merge best video + best audio without constraints
public class YtDlVideoOptionSetMergeFlexible : Lazy<OptionSet>
{
    public YtDlVideoOptionSetMergeFlexible() : base(new OptionSet
    {
        MergeOutputFormat = DownloadMergeFormat.Mp4,
        // Try merging best video + best audio without constraints
        Format = "bestvideo+bestaudio/best",
        EmbedThumbnail = true,
        Cookies = "/tmp/cookies/cookies.txt",
    })
    { }
}

// Fallback 2: Download without thumbnail to reduce processing
public class YtDlVideoOptionSetNoThumbnail : Lazy<OptionSet>
{
    public YtDlVideoOptionSetNoThumbnail() : base(new OptionSet
    {
        MergeOutputFormat = DownloadMergeFormat.Mp4,
        // Download without embedding thumbnail to reduce FFmpeg processing
        Format = "best",
        EmbedThumbnail = false,
        Cookies = "/tmp/cookies/cookies.txt",
    })
    { }
}

public class YtDlMp3OptionSet : Lazy<OptionSet>
{
    public YtDlMp3OptionSet() : base(new OptionSet
    {
        ExtractAudio = true,
        AudioFormat = AudioConversionFormat.Mp3,
        Format = "bestaudio",
        Cookies = "/tmp/cookies/cookies.txt",
    })
    { }
}
