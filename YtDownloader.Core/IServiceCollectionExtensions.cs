using Microsoft.Extensions.DependencyInjection;

using YtDownloader.Core.Services;

namespace YtDownloader.Core;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddYtDownloaderServices(this IServiceCollection services)
    {
        services.AddSingleton<YtDlVideoOptionSet>();
        services.AddSingleton<YtDlVideoOptionSetMergeFlexible>();
        services.AddSingleton<YtDlVideoOptionSetNoThumbnail>();
        services.AddSingleton<YtDlMp3OptionSet>();
        services.AddSingleton<IYtDlService, YtDlService>();
        services.AddScoped<IDownloadService, DownloadService>();
        services.AddScoped<IMp3DownloadService, Mp3DownloadService>();
        services.AddSingleton<YtDownloaderQueue>();
        services.AddHostedService(provider => provider.GetRequiredService<YtDownloaderQueue>());
        return services;
    }
}
