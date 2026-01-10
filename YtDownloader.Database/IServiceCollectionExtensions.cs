using FluentMigrator.Runner;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using YtDownloader.Base.Repositories;
using YtDownloader.Database.Repositories;

namespace YtDownloader.Database;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddYtDownloaderDatabase(this IServiceCollection services)
    {
        var dbConfig = new
        {
            Host = Environment.GetEnvironmentVariable("DATABASE_HOST"),
            User = Environment.GetEnvironmentVariable("DATABASE_USER"),
            Password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD"),
            Name = Environment.GetEnvironmentVariable("DATABASE_NAME")
        };
        //var dbConnectionString = $"server={dbConfig.Host};user id={dbConfig.User};password={dbConfig.Password};database={dbConfig.Name}";
        var dbConnectionString = $"Host={dbConfig.Host};Username={dbConfig.User};Password={dbConfig.Password};Database={dbConfig.Name}";

        var maxRetries = 10;
        var retryDelay = TimeSpan.FromSeconds(5);

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using var connection = new NpgsqlConnection($"Host={dbConfig.Host};Username={dbConfig.User};Password={dbConfig.Password};Database=postgres");
                connection.Open();
                Console.WriteLine("✅ Connected to database!");
                var command = connection.CreateCommand();
                command.CommandText = $"CREATE DATABASE {dbConfig.Name}";
                command.ExecuteNonQuery();
                Console.WriteLine($"✅ Database {dbConfig.Name} created!");
                break;
            }
            catch (NpgsqlException ex) when (ex.SqlState == "42P04") // Database already exists
            {
                Console.WriteLine($"✅ Database {dbConfig.Name} already exists!");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⏳ Try {i + 1} wasn't successfull: {ex.Message}");
                Thread.Sleep(retryDelay);
            }
        }

        services.AddDbContextPool<YtDownloaderContext>(options => options
            .UseNpgsql(dbConnectionString));
        services.AddScoped<IDownloadRepository, DownloadEntityRepository>();
        services.AddScoped<IOptionSetRepository, OptionSetRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(dbConnectionString)
                .ScanIn(typeof(IServiceCollectionExtensions).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        return services;
    }
}