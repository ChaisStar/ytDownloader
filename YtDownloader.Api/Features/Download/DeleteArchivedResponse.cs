namespace YtDownloader.Api.Features.Download;

public record DeleteArchivedResponse
{
    public string? Message { get; set; }
    public int DeletedCount { get; set; }
}
