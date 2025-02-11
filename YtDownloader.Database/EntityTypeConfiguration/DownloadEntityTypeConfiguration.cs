using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using YtDownloader.Database.Entities;

namespace YtDownloader.Database.EntityTypeConfiguration;

internal class DownloadEntityTypeConfiguration : IEntityTypeConfiguration<DownloadEntity>
{
    public void Configure(EntityTypeBuilder<DownloadEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Url).IsUnique();
        builder.Property(x => x.Url).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Created).IsRequired();
        builder.Property(x => x.Later).IsRequired();
    }
}