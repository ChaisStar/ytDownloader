using Microsoft.EntityFrameworkCore;

using YtDownloader.Database.Entities;

namespace YtDownloader.Database;

public class YtDownloaderContext(DbContextOptions<YtDownloaderContext> contextOptions) : DbContext(contextOptions)
{
    internal virtual DbSet<DownloadEntity> Downloads { get; set; }
    internal virtual DbSet<DownloadOptionSetEntity> OptionSets { get; set; }
    internal virtual DbSet<TagEntity> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        base.OnModelCreating(modelBuilder);
    }
}