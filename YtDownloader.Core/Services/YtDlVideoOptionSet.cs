using YoutubeDLSharp.Options;

namespace YtDownloader.Core.Services;

public class YtDlVideoOptionSet : Lazy<OptionSet>
{
    public YtDlVideoOptionSet() : base(new OptionSet
    {
        MergeOutputFormat = DownloadMergeFormat.Mp4,
        // Simpler format to avoid FFmpeg issues
        // Get best video up to 1080p + best audio, with fallbacks for compatibility
        Format = "best[height<=1080]/bestvideo[height<=1080]+bestaudio/best",
        EmbedThumbnail = true,
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
