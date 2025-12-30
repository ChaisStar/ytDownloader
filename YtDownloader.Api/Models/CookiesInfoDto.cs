namespace YtDownloader.Api.Models;

public record CookiesInfoDto
{
    public DateTime? LastModified { get; set; }
    public long Size { get; set; }
    public bool Exists { get; set; }
}
