﻿using Microsoft.Extensions.DependencyInjection;

using YtDownloader.Core.Services;

namespace YtDownloader.Core;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddYtDownloaderServices(this IServiceCollection services)
    {
        services.AddSingleton<YtDlOptionSet>();
        services.AddSingleton<IYtDlService, YtDlService>();
        services.AddScoped<IDownloadService, DownloadService>();
        services.AddSingleton<YtDownloaderQueue>();
        services.AddHostedService(provider => provider.GetRequiredService<YtDownloaderQueue>());
        return services;
    }
}
