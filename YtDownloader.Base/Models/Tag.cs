using YtDownloader.Base.Enums;

namespace YtDownloader.Base.Models;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public TagUsage Usage { get; set; }
    public string? Color { get; set; }

    public static Tag Create(int id, string name, string value, TagUsage usage, string? color) =>
        new() { Id = id, Name = name, Value = value, Usage = usage, Color = color };
}
