using YtDownloader.Base.Enums;
using YtDownloader.Base.Models;

namespace YtDownloader.Database.Entities;

internal class TagEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public TagUsage Usage { get; set; }
    public string? Color { get; set; }

    public static implicit operator Tag(TagEntity entity) =>
        Tag.Create(entity.Id, entity.Name, entity.Value, entity.Usage, entity.Color);
}
