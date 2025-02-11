using YtDownloader.Base.Enums;

namespace YtDownloader.Base.Models;

public record Download
{
    public Download(int id, string url, string? thumbnail,
        string? title, DownloadStatus status, int progress,
        long? totalSize, string? speed, string? eTA,
        DateTime created, DateTime? started,
        DateTime? finished, bool later)
    {
        Id = id;
        Url = url ?? throw new ArgumentNullException(nameof(url));
        Thumbnail = thumbnail;
        Title = title;
        Status = status;
        Progress = progress;
        TotalSize = totalSize;
        Speed = speed;
        ETA = eTA;
        Created = created;
        Started = started;
        Finished = finished;
        Later = later;
    }

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
    public bool Later { get; }

    public string[] SetInfo(string title, long totalSize, string thumbnail)
    {
        Title = title;
        Status = DownloadStatus.Pending;
        TotalSize = totalSize;
        Thumbnail = thumbnail;

        return [nameof(Title), nameof(Status), nameof(TotalSize), nameof(Thumbnail)];
    }

    public string[] Start()
    {
        Status = DownloadStatus.Downloading;
        Started = DateTime.UtcNow;
        return [nameof(Status), nameof(Started)];
    }

    public string[] Finish()
    {
        Status = DownloadStatus.Finished;
        Finished = DateTime.UtcNow;
        ETA = null;
        Speed = null;
        Progress = 100;
        return [nameof(Status), nameof(Finished), nameof(ETA), nameof(Speed), nameof(Progress)];
    }

    public string[] Fail()
    {
        Status = DownloadStatus.Failed;
        Finished = DateTime.UtcNow;
        ETA = null;
        Speed = null;
        return [nameof(Status), nameof(Finished), nameof(ETA), nameof(Speed)];
    }

    public string[] UpdateProgress(int progress, string speed, string eTA)
    {
        Progress = progress;
        Speed = speed;
        ETA = eTA;
        return [nameof(Progress), nameof(Speed), nameof(ETA)];
    }
}