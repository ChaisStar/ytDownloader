using YoutubeDLSharp.Options;

namespace YtDownloader.Base.Models;

public class DownloadOptionSet
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Format { get; set; }
    public DownloadMergeFormat? MergeOutputFormat { get; set; }
    public bool EmbedThumbnail { get; set; }
    public bool ExtractAudio { get; set; }
    public AudioConversionFormat? AudioFormat { get; set; }
    public int Priority { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsDefault { get; set; } = false; // For the initial 1080p high quality option

    public static DownloadOptionSet Create(string name, string? format, int priority, bool isEnabled = true, DownloadMergeFormat? merge = DownloadMergeFormat.Mp4, bool thumbnail = true)
    {
        return new DownloadOptionSet
        {
            Name = name,
            Format = format,
            Priority = priority,
            IsEnabled = isEnabled,
            MergeOutputFormat = merge,
            EmbedThumbnail = thumbnail
        };
    }

    public OptionSet ToYtDlOptions()
    {
        var options = new OptionSet
        {
            Format = Format,
            EmbedThumbnail = EmbedThumbnail,
            ExtractAudio = ExtractAudio,
            Cookies = "/tmp/cookies/cookies.txt"
        };

        if (MergeOutputFormat.HasValue)
        {
            options.MergeOutputFormat = MergeOutputFormat.Value;
        }

        if (AudioFormat.HasValue)
        {
            options.AudioFormat = AudioFormat.Value;
        }

        return options;
    }
}
