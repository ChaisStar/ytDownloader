using FluentMigrator;

namespace YtDownloader.Database.Migrations;

[Migration(2026_01_11_15_00, "Migrate Later boolean to Tags and remove column")]
public class RemoveLaterColumn : Migration
{
    public override void Up()
    {
        // 1. Ensure we have the "Watch Later" tag ID
        // We'll use a subquery to find it, or fallback to the seed if we know it exists.
        // If it doesn't exist for some reason, we won't update anything.
        
        Execute.Sql(@"
            UPDATE ""Downloads"" 
            SET ""TagId"" = (SELECT ""Id"" FROM ""Tags"" WHERE ""Value"" = 'youtube_later' LIMIT 1)
            WHERE ""Later"" = true AND ""TagId"" IS NULL;
        ");

        Delete.Column("Later").FromTable("Downloads");
    }

    public override void Down()
    {
        Alter.Table("Downloads")
            .AddColumn("Later").AsBoolean().NotNullable().WithDefaultValue(false);
            
        // We can't easily reverse the tag back to boolean perfectly if user had both, 
        // but we can try to set Later=true where Tag matches "Watch Later"
        Execute.Sql(@"
            UPDATE ""Downloads""
            SET ""Later"" = true
            WHERE ""TagId"" IN (SELECT ""Id"" FROM ""Tags"" WHERE ""Name"" = 'Watch Later');
        ");
    }
}
