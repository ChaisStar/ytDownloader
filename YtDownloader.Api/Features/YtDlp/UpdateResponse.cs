namespace YtDownloader.Api.Features.YtDlp;

public record UpdateResponse
{
    public string? Message { get; set; }
    public string? Output { get; set; }
}
