using FluentMigrator;

using YtDownloader.Database.Entities;

namespace YtDownloader.Database.Migrations;

[Migration(2025_02_13_00_47, "Added table 'Downloads'")]
public class _20250213_0047_AddDownloadEntitiesTable: Migration
{
    public override void Up()
    {
        Create.Table("Downloads")
            .WithColumn(nameof(DownloadEntity.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(DownloadEntity.Url)).AsString(256).Unique().NotNullable()
            .WithColumn(nameof(DownloadEntity.Thumbnail)).AsString(2048).Nullable()
            .WithColumn(nameof(DownloadEntity.Title)).AsString(256).Nullable()
            .WithColumn(nameof(DownloadEntity.Status)).AsInt32().NotNullable()
            .WithColumn(nameof(DownloadEntity.Progress)).AsInt32().Nullable()
            .WithColumn(nameof(DownloadEntity.TotalSize)).AsInt64().Nullable()
            .WithColumn(nameof(DownloadEntity.Speed)).AsString(256).Nullable()
            .WithColumn(nameof(DownloadEntity.ETA)).AsString(256).Nullable()
            .WithColumn(nameof(DownloadEntity.Created)).AsDateTime().NotNullable()
            .WithColumn(nameof(DownloadEntity.Started)).AsDateTime().Nullable()
            .WithColumn(nameof(DownloadEntity.Finished)).AsDateTime().Nullable()
            .WithColumn(nameof(DownloadEntity.Later)).AsBoolean().NotNullable();
    }
    public override void Down() => Delete.Table("Downloads");
}
