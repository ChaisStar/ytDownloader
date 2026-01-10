using YoutubeDLSharp.Options;

namespace YtDownloader.Api.Models;

public class OptionSetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Format { get; set; }
    public DownloadMergeFormat? MergeOutputFormat { get; set; }
    public bool EmbedThumbnail { get; set; }
    public bool IsEnabled { get; set; }
    public int Priority { get; set; }
    public bool ExtractAudio { get; set; }
    public AudioConversionFormat? AudioFormat { get; set; }
}

public class UpdatePrioritiesRequest
{
    public List<PriorityEntry> Priorities { get; set; } = [];
}

public class PriorityEntry
{
    public int Id { get; set; }
    public int Priority { get; set; }
}
