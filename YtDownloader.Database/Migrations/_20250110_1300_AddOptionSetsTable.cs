using FluentMigrator;
using YtDownloader.Database.Entities;
using YoutubeDLSharp.Options;

namespace YtDownloader.Database.Migrations;

[Migration(2025_01_10_13_00, "Added table 'OptionSets'")]
public class AddOptionSetsTable : Migration
{
    public override void Up()
    {
        Create.Table("OptionSets")
            .WithColumn(nameof(DownloadOptionSetEntity.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(DownloadOptionSetEntity.Name)).AsString(256).NotNullable()
            .WithColumn(nameof(DownloadOptionSetEntity.Format)).AsString(1024).Nullable()
            .WithColumn(nameof(DownloadOptionSetEntity.MergeOutputFormat)).AsInt32().Nullable()
            .WithColumn(nameof(DownloadOptionSetEntity.EmbedThumbnail)).AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn(nameof(DownloadOptionSetEntity.ExtractAudio)).AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn(nameof(DownloadOptionSetEntity.AudioFormat)).AsInt32().Nullable()
            .WithColumn(nameof(DownloadOptionSetEntity.Priority)).AsInt32().NotNullable()
            .WithColumn(nameof(DownloadOptionSetEntity.IsEnabled)).AsBoolean().NotNullable().WithDefaultValue(true);

        // Seed with existing options
        Insert.IntoTable("OptionSets").Row(new
        {
            Name = "main",
            Format = "bestvideo[height<=1080][height>=720][fps<=30]+bestaudio[ext=m4a][abr<=128]/bestvideo[height<=1080][fps<=30]+bestaudio[ext=m4a]/best[height<=1080]",
            MergeOutputFormat = (int)DownloadMergeFormat.Mp4,
            EmbedThumbnail = true,
            Priority = 0,
            IsEnabled = true
        });

        Insert.IntoTable("OptionSets").Row(new
        {
            Name = "primary",
            Format = "bestvideo[height<=1080][height>=720][fps<=30]+bestaudio[ext=m4a][abr<=128]/bestvideo[height<=1080][fps<=30]+bestaudio[ext=m4a]/best[height<=1080]",
            MergeOutputFormat = (int)DownloadMergeFormat.Mp4,
            EmbedThumbnail = true,
            Priority = 1,
            IsEnabled = true
        });

        Insert.IntoTable("OptionSets").Row(new
        {
            Name = "flexible merge",
            Format = "bestvideo[ext=mp4]+bestaudio[ext=m4a]/best",
            MergeOutputFormat = (int)DownloadMergeFormat.Mp4,
            EmbedThumbnail = true,
            Priority = 2,
            IsEnabled = true
        });

        Insert.IntoTable("OptionSets").Row(new
        {
            Name = "without thumbnail",
            Format = "bestvideo+bestaudio/best",
            MergeOutputFormat = (int)DownloadMergeFormat.Mp4,
            EmbedThumbnail = false,
            Priority = 3,
            IsEnabled = true
        });

        Insert.IntoTable("OptionSets").Row(new
        {
            Name = "auto-merge",
            MergeOutputFormat = (int)DownloadMergeFormat.Mp4,
            EmbedThumbnail = false,
            Priority = 4,
            IsEnabled = true
        });

        Insert.IntoTable("OptionSets").Row(new
        {
            Name = "best pre-merged",
            Format = "b",
            MergeOutputFormat = (int)DownloadMergeFormat.Mp4,
            EmbedThumbnail = false,
            Priority = 5,
            IsEnabled = true
        });

        Insert.IntoTable("OptionSets").Row(new
        {
            Name = "raw download",
            EmbedThumbnail = false,
            Priority = 6,
            IsEnabled = true
        });

        Insert.IntoTable("OptionSets").Row(new
        {
            Name = "video-only",
            Format = "bestvideo[ext=mp4]/best",
            EmbedThumbnail = false,
            Priority = 7,
            IsEnabled = true
        });

        Insert.IntoTable("OptionSets").Row(new
        {
            Name = "mp3 default",
            Format = "bestaudio",
            ExtractAudio = true,
            AudioFormat = (int)AudioConversionFormat.Mp3,
            Priority = 100, // High priority so it doesn't interfere with video fallbacks if we ever mix them
            IsEnabled = true
        });
    }

    public override void Down() => Delete.Table("OptionSets");
}
