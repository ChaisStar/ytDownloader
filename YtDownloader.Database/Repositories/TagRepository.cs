using Microsoft.EntityFrameworkCore;
using YtDownloader.Base.Models;
using YtDownloader.Base.Repositories;
using YtDownloader.Database.Entities;
using YtDownloader.Base.Enums;

namespace YtDownloader.Database.Repositories;

internal class TagRepository(YtDownloaderContext context) : ITagRepository
{
    public async Task<IReadOnlyList<Tag>> GetAll()
    {
        var entities = await context.Tags.ToListAsync();
        return entities.Select(e => (Tag)e).ToList();
    }

    public async Task<Tag?> GetById(int id)
    {
        var entity = await context.Tags.FindAsync(id);
        return entity != null ? (Tag)entity : null;
    }

    public async Task<Tag> Create(string name, string value, TagUsage usage, string? color)
    {
        var entity = new TagEntity
        {
            Name = name,
            Value = value,
            Usage = usage,
            Color = color
        };
        context.Tags.Add(entity);
        await context.SaveChangesAsync();
        return (Tag)entity;
    }

    public async Task Update(Tag tag)
    {
        var entity = await context.Tags.FindAsync(tag.Id);
        if (entity == null) return;

        entity.Name = tag.Name;
        entity.Value = tag.Value;
        entity.Usage = tag.Usage;
        entity.Color = tag.Color;

        await context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var entity = await context.Tags.FindAsync(id);
        if (entity == null) return;

        context.Tags.Remove(entity);
        await context.SaveChangesAsync();
    }
}
