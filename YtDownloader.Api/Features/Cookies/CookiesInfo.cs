namespace YtDownloader.Api.Features.Cookies;

public record CookiesInfo
{
    public DateTime? LastModified { get; set; }
    public long Size { get; set; }
    public bool Exists { get; set; }
}
