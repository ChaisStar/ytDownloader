using YtDownloader.Base.Enums;
using YtDownloader.Base.Models;

namespace YtDownloader.Api.Models;

public class DownloadResponse(Download download)
{
    public int Id { get; } = download.Id;
    public string Url { get; } = download.Url;
    public string? Thumbnail { get; private set; } = download.Thumbnail;
    public string? Title { get; private set; } = download.Title;
    public DownloadStatus Status { get; private set; } = download.Status;
    public int Progress { get; private set; } = download.Progress;
    public long? TotalSize { get; private set; } = download.TotalSize;
    public string? Speed { get; private set; } = download.Speed;
    public string? ETA { get; private set; } = download.ETA;
    public DateTime Created { get; private set; } = download.Created;
    public DateTime? Started { get; private set; } = download.Started;
    public DateTime? Finished { get; private set; } = download.Finished;
    public bool Later { get; } = download.Later;
    public string? ErrorMessage { get; private set; } = download.ErrorMessage;
}
