using FluentMigrator;

using YtDownloader.Database.Entities;

namespace YtDownloader.Database.Migrations;

[Migration(202501050000, "Added ErrorMessage column to Downloads table")]
public class AddErrorMessageToDownloadsTable : Migration
{
    public override void Up()
    {
        Alter.Table("Downloads")
            .AddColumn(nameof(DownloadEntity.ErrorMessage)).AsString(4096).Nullable();
    }

    public override void Down()
    {
        Delete.Column(nameof(DownloadEntity.ErrorMessage)).FromTable("Downloads");
    }
}
