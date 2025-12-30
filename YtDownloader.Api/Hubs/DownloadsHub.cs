using Microsoft.AspNetCore.SignalR;

using YtDownloader.Api.Models;

namespace YtDownloader.Api.Hubs;

public class DownloadsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendDownloadsUpdate(ICollection<DownloadResponse> downloads)
    {
        await Clients.All.SendAsync("ReceiveDownloadsUpdate", downloads);
    }

    public async Task SendVersionUpdate(string version)
    {
        await Clients.All.SendAsync("ReceiveVersionUpdate", version);
    }

    public async Task SendCookiesUpdate(CookiesInfoDto cookiesInfo)
    {
        await Clients.All.SendAsync("ReceiveCookiesUpdate", cookiesInfo);
    }
}
