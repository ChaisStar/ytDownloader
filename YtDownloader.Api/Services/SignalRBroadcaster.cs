using Microsoft.AspNetCore.SignalR;
using YtDownloader.Api.Hubs;
using YtDownloader.Api.Models;

namespace YtDownloader.Api.Services;

public interface ISignalRBroadcaster
{
    Task BroadcastDownloadsUpdate(ICollection<DownloadResponse> downloads);
    Task BroadcastVersionUpdate(string version);
    Task BroadcastCookiesUpdate(CookiesInfoDto cookiesInfo);
}

public class SignalRBroadcaster(IHubContext<DownloadsHub> hubContext) : ISignalRBroadcaster
{
    public async Task BroadcastDownloadsUpdate(ICollection<DownloadResponse> downloads)
    {
        try
        {
            await hubContext.Clients.All.SendAsync("ReceiveDownloadsUpdate", downloads);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error broadcasting downloads update: {ex.Message}");
        }
    }

    public async Task BroadcastVersionUpdate(string version)
    {
        try
        {
            await hubContext.Clients.All.SendAsync("ReceiveVersionUpdate", version);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error broadcasting version update: {ex.Message}");
        }
    }

    public async Task BroadcastCookiesUpdate(CookiesInfoDto cookiesInfo)
    {
        try
        {
            await hubContext.Clients.All.SendAsync("ReceiveCookiesUpdate", cookiesInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error broadcasting cookies update: {ex.Message}");
        }
    }
}
