using FluentMigrator;
using YtDownloader.Database.Entities;
using YtDownloader.Base.Enums;

namespace YtDownloader.Database.Migrations;

[Migration(2026_01_11_14_00, "Added Tags table and TagId to DownloadEntities")]
public class AddTagsTable : Migration
{
    public override void Up()
    {
        Create.Table("Tags")
            .WithColumn(nameof(TagEntity.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(TagEntity.Name)).AsString(256).NotNullable()
            .WithColumn(nameof(TagEntity.Value)).AsString(256).NotNullable()
            .WithColumn(nameof(TagEntity.Usage)).AsInt32().NotNullable()
            .WithColumn(nameof(TagEntity.Color)).AsString(50).Nullable();

        Alter.Table("Downloads")
            .AddColumn(nameof(DownloadEntity.TagId)).AsInt32().Nullable().ForeignKey("FK_Downloads_Tags", "Tags", "Id");

        // Seed "Watch Later" tag
        Insert.IntoTable("Tags").Row(new
        {
            Name = "Watch Later",
            Value = "youtube_later",
            Usage = (int)TagUsage.Directory,
            Color = "#f43f5e" 
        });
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_Downloads_Tags").OnTable("Downloads");
        Delete.Column(nameof(DownloadEntity.TagId)).FromTable("Downloads");
        Delete.Table("Tags");
    }
}
