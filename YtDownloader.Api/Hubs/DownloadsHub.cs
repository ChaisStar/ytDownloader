using Microsoft.AspNetCore.SignalR;

using YtDownloader.Api.Models;

namespace YtDownloader.Api.Hubs;

public class DownloadsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Client connected: {Context.ConnectionId} from {Context.GetHttpContext()?.Connection.RemoteIpAddress}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var msg = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Client disconnected: {Context.ConnectionId}";
        if (exception != null)
            msg += $" - Exception: {exception.Message}";
        Console.WriteLine(msg);
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
