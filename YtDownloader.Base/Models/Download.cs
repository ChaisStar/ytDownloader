using YtDownloader.Base.Enums;

namespace YtDownloader.Base.Models;

public class Download
{
    public int Id { get; }
    public string Url { get; }
    public string? Thumbnail { get; private set; }
    public string? Title { get; private set; }
    public DownloadStatus Status { get; private set; }
    public int Progress { get; private set; }
    public long? TotalSize { get; private set; }
    public string? Speed { get; private set; }
    public string? ETA { get; private set; }
    public DateTime Created { get; private set; }
    public DateTime? Started { get; private set; }
    public DateTime? Finished { get; private set; }
    public int? TagId { get; private set; }
    public Tag? Tag { get; set; }
    public string? ErrorMessage { get; private set; }
    public int Retries { get; private set; }

    public Download(int id, string url, int? tagId = null)
    {
        Id = id;
        Url = url ?? throw new ArgumentNullException(nameof(url));
        TagId = tagId;
        Status = DownloadStatus.Pending;
        Progress = 0;
        Created = DateTime.UtcNow;
        ErrorMessage = null;
        Retries = 0;
    }

    // Factory method to reconstruct Download from database record. Multiple parameters are necessary to restore full state.
#pragma warning disable S107 // Methods should not have too many parameters
    public static Download CreateFromDatabase(int id, string url, string? thumbnail, string? title,
        DownloadStatus status, int progress, long? totalSize, string? speed,
        string? eTA, DateTime created, DateTime? started, DateTime? finished,
        string? errorMessage, int retries = 0, int? tagId = null, Tag? tag = null)
#pragma warning restore S107
    {
        #pragma warning disable CS8601
        return new Download(id, url, tagId)
        #pragma warning restore CS8601
        {
            Thumbnail = thumbnail,
            Title = title,
            Tag = tag,
            Status = status,
            Progress = progress,
            TotalSize = totalSize,
            Speed = speed,
            ETA = eTA,
            Created = created,
            Started = started,
            Finished = finished,
            ErrorMessage = errorMessage,
            Retries = retries
        };
    }

    public string[] SetInfo(string title, long totalSize, string thumbnail)
    {
        Thumbnail = thumbnail;
        Title = title;
        Status = DownloadStatus.Pending;
        TotalSize = totalSize;
        // Don't clear ErrorMessage - preserve it in case download is retried
        return [nameof(Title), nameof(Status), nameof(TotalSize), nameof(Thumbnail)];
    }

    public string[] Start()
    {
        Status = DownloadStatus.Downloading;
        Started = DateTime.UtcNow;
        // Don't clear ErrorMessage - include it explicitly to preserve during retry
        return [nameof(Status), nameof(Started), nameof(ErrorMessage)];
    }

    public string[] Finish(long fileSize)
    {
        Status = DownloadStatus.Finished;
        Finished = DateTime.UtcNow;
        ETA = null;
        Speed = null;
        Progress = 100;
        TotalSize = fileSize;
        ErrorMessage = null; // Clear error on success
        return [nameof(Status), nameof(Finished), nameof(ETA), nameof(Speed), nameof(Progress), nameof(TotalSize), nameof(ErrorMessage)];
    }

    public string[] Fail(string? errorMessage = null)
    {
        Status = DownloadStatus.Failed;
        Finished = DateTime.UtcNow;
        ETA = null;
        Speed = null;
        ErrorMessage = errorMessage;
        return [nameof(Status), nameof(Finished), nameof(ETA), nameof(Speed), nameof(ErrorMessage)];
    }

    public string[] UpdateProgress(int progress, string speed, string eTA)
    {
        Status = DownloadStatus.Downloading;
        Progress = progress;
        Speed = speed;
        ETA = eTA;
        return [nameof(Progress), nameof(Speed), nameof(ETA), nameof(Status)];
    }

    public string[] IncrementRetries()
    {
        Retries++;
        return [nameof(Retries)];
    }
}