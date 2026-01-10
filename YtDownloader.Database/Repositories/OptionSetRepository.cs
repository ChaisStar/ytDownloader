using Microsoft.EntityFrameworkCore;
using YtDownloader.Base.Models;
using YtDownloader.Base.Repositories;
using YtDownloader.Database.Entities;

namespace YtDownloader.Database.Repositories;

internal class OptionSetRepository(YtDownloaderContext context) : IOptionSetRepository
{
    public async Task<IReadOnlyList<DownloadOptionSet>> GetAll()
    {
        var entities = await context.OptionSets
            .OrderBy(o => o.Priority)
            .ToListAsync();
        return entities.Select(e => (DownloadOptionSet)e).ToList();
    }

    public async Task<IReadOnlyList<DownloadOptionSet>> GetEnabled()
    {
        var entities = await context.OptionSets
            .Where(o => o.IsEnabled)
            .OrderBy(o => o.Priority)
            .ToListAsync();
        return entities.Select(e => (DownloadOptionSet)e).ToList();
    }

    public async Task<DownloadOptionSet?> GetById(int id)
    {
        var entity = await context.OptionSets.FindAsync(id);
        return entity == null ? null : (DownloadOptionSet)entity;
    }

    public async Task Update(DownloadOptionSet optionSet)
    {
        var entity = await context.OptionSets.FindAsync(optionSet.Id);
        if (entity != null)
        {
            entity.Name = optionSet.Name;
            entity.Format = optionSet.Format;
            entity.MergeOutputFormat = optionSet.MergeOutputFormat;
            entity.EmbedThumbnail = optionSet.EmbedThumbnail;
            entity.ExtractAudio = optionSet.ExtractAudio;
            entity.AudioFormat = optionSet.AudioFormat;
            entity.Priority = optionSet.Priority;
            entity.IsEnabled = optionSet.IsEnabled;
            
            await context.SaveChangesAsync();
        }
    }

    public async Task<int> Create(DownloadOptionSet optionSet)
    {
        var entity = DownloadOptionSetEntity.FromModel(optionSet);
        context.OptionSets.Add(entity);
        await context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task Delete(int id)
    {
        var entity = await context.OptionSets.FindAsync(id);
        if (entity != null)
        {
            context.OptionSets.Remove(entity);
            await context.SaveChangesAsync();
        }
    }

    public async Task UpdatePriorities(IEnumerable<(int Id, int Priority)> priorities)
    {
        foreach (var (id, priority) in priorities)
        {
            var entity = await context.OptionSets.FindAsync(id);
            if (entity != null)
            {
                entity.Priority = priority;
            }
        }
        await context.SaveChangesAsync();
    }
}
