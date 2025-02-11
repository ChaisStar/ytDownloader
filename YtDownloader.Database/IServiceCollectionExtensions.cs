using FluentMigrator.Runner;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using MySqlConnector;

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
        var dbConnectionString = $"server={dbConfig.Host};user id={dbConfig.User};password={dbConfig.Password};database={dbConfig.Name}";

        var maxRetries = 10;
        var retryDelay = TimeSpan.FromSeconds(5);

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using var connection = new MySqlConnection($"server={dbConfig.Host};user id={dbConfig.User};password={dbConfig.Password}");
                connection.Open();
                Console.WriteLine("✅ Connected to database!");
                var command = connection.CreateCommand();
                command.CommandText = $"CREATE DATABASE IF NOT EXISTS {dbConfig.Name}";
                command.ExecuteNonQuery();
                Console.WriteLine($"✅ Database {dbConfig.Name} created!");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⏳ Try {i + 1} wasn't successfull: {ex.Message}");
                Thread.Sleep(retryDelay);
            }
        }

        services.AddDbContextPool<YtDownloaderContext>(options => options
            .UseMySql(dbConnectionString, ServerVersion.AutoDetect(dbConnectionString)));
        services.AddScoped<IDownloadRepository, DownloadEntityRepository>();

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddMySql8()
                .WithGlobalConnectionString(dbConnectionString)
                .ScanIn(typeof(IServiceCollectionExtensions).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        return services;
    }
}