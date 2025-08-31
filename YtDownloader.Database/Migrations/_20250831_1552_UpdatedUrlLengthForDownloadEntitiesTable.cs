using FluentMigrator;

using YtDownloader.Database.Entities;

namespace YtDownloader.Database.Migrations;

[Migration(202508311552, "Updated URL length for table 'Downloads'")]
public class _20250831_1552_UpdatedUrlLengthForDownloadEntitiesTable : Migration
{
    public override void Up() =>
        Alter.Table("Downloads")
             .AlterColumn(nameof(DownloadEntity.Url))
             .AsString(2048)
             .NotNullable();

    public override void Down() =>
        Alter.Table("Downloads")
             .AlterColumn(nameof(DownloadEntity.Url))
             .AsString(256)
             .NotNullable();
}