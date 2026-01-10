using YtDownloader.Base.Enums;

namespace YtDownloader.Api.Models;

public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public TagUsage Usage { get; set; }
    public string? Color { get; set; }
}
