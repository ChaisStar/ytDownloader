using YtDownloader.Base.Models;
using YoutubeDLSharp.Options;

namespace YtDownloader.Database.Entities;

internal class DownloadOptionSetEntity
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

    public static implicit operator DownloadOptionSet(DownloadOptionSetEntity entity) => new DownloadOptionSet
    {
        Id = entity.Id,
        Name = entity.Name,
        Format = entity.Format,
        MergeOutputFormat = entity.MergeOutputFormat,
        EmbedThumbnail = entity.EmbedThumbnail,
        ExtractAudio = entity.ExtractAudio,
        AudioFormat = entity.AudioFormat,
        Priority = entity.Priority,
        IsEnabled = entity.IsEnabled
    };

    public static DownloadOptionSetEntity FromModel(DownloadOptionSet model) => new DownloadOptionSetEntity
    {
        Id = model.Id,
        Name = model.Name,
        Format = model.Format,
        MergeOutputFormat = model.MergeOutputFormat,
        EmbedThumbnail = model.EmbedThumbnail,
        ExtractAudio = model.ExtractAudio,
        AudioFormat = model.AudioFormat,
        Priority = model.Priority,
        IsEnabled = model.IsEnabled
    };
}
