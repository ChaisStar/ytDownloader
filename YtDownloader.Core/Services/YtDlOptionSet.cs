using YoutubeDLSharp.Options;

namespace YtDownloader.Core.Services;

public class YtDlOptionSet : Lazy<OptionSet>
{
    public YtDlOptionSet() : base(new OptionSet
    {
        MergeOutputFormat = DownloadMergeFormat.Mp4,
        Format = "bestvideo+bestaudio[ext=m4a]/best",
        Verbose = true
    })
    { }
}
