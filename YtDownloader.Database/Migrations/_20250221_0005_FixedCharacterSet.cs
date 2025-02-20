using FluentMigrator;

namespace YtDownloader.Database.Migrations;

[Migration(2025_02_21_00_05, "Fixed character set issue on 'Downloads'")]
public class _20250221_0005_FixedCharacterSet : Migration
{
    public override void Up()
    {
        Execute.Sql("ALTER DATABASE ytDownload CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");
        Execute.Sql("ALTER TABLE Downloads CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");
    }

    public override void Down() { }
}
