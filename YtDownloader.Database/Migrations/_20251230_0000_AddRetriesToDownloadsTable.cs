using FluentMigrator;

using YtDownloader.Database.Entities;

namespace YtDownloader.Database.Migrations;

[Migration(202512300000, "Added Retries column to Downloads table")]
public class AddRetriesToDownloadsTable : Migration
{
    public override void Up()
    {
        Alter.Table("Downloads")
            .AddColumn(nameof(DownloadEntity.Retries)).AsInt32().WithDefaultValue(0);
    }

    public override void Down()
    {
        Delete.Column(nameof(DownloadEntity.Retries)).FromTable("Downloads");
    }
}
