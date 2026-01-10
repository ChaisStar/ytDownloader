using Microsoft.EntityFrameworkCore;

using System.Data;

using YtDownloader.Base.Enums;
using YtDownloader.Base.Models;
using YtDownloader.Base.Repositories;
using YtDownloader.Database.Entities;

namespace YtDownloader.Database.Repositories;

internal class DownloadEntityRepository(YtDownloaderContext context) : IDownloadRepository
{
    public async Task<Download> Create(string url, int? tagId = null)
    {
        var existing = await context.Downloads.Include(d => d.Tag).FirstOrDefaultAsync(d => d.Url == url);
        if (existing is not null)
        {
            return existing;
        }
        var download = new DownloadEntity()
        {
            Url = url,
            TagId = tagId,
            Created = DateTime.UtcNow
        };
        context.Downloads.Add(download);
        await context.SaveChangesAsync();
        return await Get(download.Id);
    }

    public async Task<Download> Get(int id) => await context.Downloads.Include(d => d.Tag).FirstOrDefaultAsync(d => d.Id == id) ?? throw new ArgumentException("Not found");

    public async Task<Download> Get(string url) => await context.Downloads.Include(d => d.Tag).FirstOrDefaultAsync(d => d.Url == url) ?? throw new ArgumentException("Not found");

    public async Task<IReadOnlyList<Download>> Get(DateTime from, DateTime? to = null)
    {
        var query = context.Downloads.Include(d => d.Tag).Where(d => d.Created >= from.Date);
        if (to is not null)
        {
            query = query.Where(d => d.Created < to.Value.Date.AddDays(1).Date);
        }
        var downloads = await query.ToListAsync();
        return downloads.Select(d => (Download)d).ToList();
    }

    public async Task<IReadOnlyList<Download>> Get(params DownloadStatus[] downloadStatuses)
    {
        var query = context.Downloads.Include(d => d.Tag).Where(d => downloadStatuses.Contains(d.Status));
        var downloads = await query.ToListAsync();
        return downloads.Select(d => (Download)d).ToList();
    }

    public async Task<IReadOnlyList<Download>> GetUndefined()
    {
        var query = context.Downloads.Include(d => d.Tag).Where(d => d.Title == null && d.Status != DownloadStatus.Pending);
        var downloads = await query.ToListAsync();
        return downloads.Select(d => (Download)d).ToList();
    }

    public async Task Remove(int id)
    {
        var download = await context.Downloads.FindAsync(id);
        if (download is null)
            return;

        context.Downloads.Remove(download);
        await context.SaveChangesAsync();
    }

    public async Task<Download> Update(Download download, string[] columnsToUpdate)
    {
        using var transaction = context.Database.BeginTransaction(IsolationLevel.Serializable);
        var entity = await context.Downloads.FindAsync(download.Id);
        if (entity is null)
            throw new ArgumentException("Donwload not found", nameof(download));

        foreach (var column in columnsToUpdate)
        {
            entity.GetType().GetProperty(column)?.SetValue(entity, download.GetType().GetProperty(column)?.GetValue(download));
        }

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return entity;
    }
}
