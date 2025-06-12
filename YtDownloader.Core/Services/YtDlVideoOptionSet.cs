using YoutubeDLSharp.Options;

namespace YtDownloader.Core.Services;

public class YtDlVideoOptionSet : Lazy<OptionSet>
{
    public YtDlVideoOptionSet() : base(new OptionSet
    {
        MergeOutputFormat = DownloadMergeFormat.Mp4,
        //Format = "bestvideo+bestaudio[ext=m4a]/best"
        //Format = "bestvideo[height<=1080][height>=720]+bestaudio[ext=m4a][abr<=128]/bestvideo[height<=1080]+bestaudio/best[height<=1080]",
        Format = "bestvideo[height<=1080][height>=720][fps<=30]+bestaudio[ext=m4a][abr<=128]/bestvideo[height<=1080][fps<=30]+bestaudio[ext=m4a]/best[height<=1080]",
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
