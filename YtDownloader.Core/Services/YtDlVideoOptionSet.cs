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
        // Try merging best video + best audio without height/fps constraints
        Format = "bestvideo[ext=mp4]+bestaudio[ext=m4a]/best",
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
        Format = "bestvideo+bestaudio/best",
        EmbedThumbnail = false,
        Cookies = "/tmp/cookies/cookies.txt",
    })
    { }
}

// Fallback 3: Let yt-dlp auto-merge without any format constraints
public class YtDlVideoOptionSetAutoMerge : Lazy<OptionSet>
{
    public YtDlVideoOptionSetAutoMerge() : base(new OptionSet
    {
        MergeOutputFormat = DownloadMergeFormat.Mp4,
        // No format restriction - let yt-dlp choose the best available format
        EmbedThumbnail = false,
        Cookies = "/tmp/cookies/cookies.txt",
    })
    { }
}

// Fallback 4: Best pre-merged format (no merging needed)
public class YtDlVideoOptionSetBestPreMerged : Lazy<OptionSet>
{
    public YtDlVideoOptionSetBestPreMerged() : base(new OptionSet
    {
        MergeOutputFormat = DownloadMergeFormat.Mp4,
        // Use "-f b" to get best pre-merged format without attempting to merge
        Format = "b",
        EmbedThumbnail = false,
        Cookies = "/tmp/cookies/cookies.txt",
    })
    { }
}

// Fallback 5: Download raw without any post-processing
public class YtDlVideoOptionSetRawDownload : Lazy<OptionSet>
{
    public YtDlVideoOptionSetRawDownload() : base(new OptionSet
    {
        // No format, no merging, no post-processing - just download the best available single file
        EmbedThumbnail = false,
        KeepVideoFiles = true, // Keep downloaded files even if merge fails
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
