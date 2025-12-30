using YtDownloader.Base.Enums;
using YtDownloader.Base.Models;

namespace YtDownloader.Database.Entities;

internal class DownloadEntity
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Thumbnail { get; set; }
    public string? Title { get; set; }
    public DownloadStatus Status { get; set; } = DownloadStatus.Pending;
    public int Progress { get; set; } = 0;
    public long? TotalSize { get; set; }
    public string? Speed { get; set; }
    public string? ETA { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Started { get; set; }
    public DateTime? Finished { get; set; }
    public bool Later { get; set; } = false;
    public string? ErrorMessage { get; set; }
    public int Retries { get; set; } = 0;

    public static implicit operator Download(DownloadEntity downloadEntity) =>
        Download.CreateFromDatabase(downloadEntity.Id, downloadEntity.Url, downloadEntity.Thumbnail, downloadEntity.Title, downloadEntity.Status,
            downloadEntity.Progress, downloadEntity.TotalSize, downloadEntity.Speed, downloadEntity.ETA,
            downloadEntity.Created, downloadEntity.Started, downloadEntity.Finished, downloadEntity.Later, downloadEntity.ErrorMessage, downloadEntity.Retries);
}