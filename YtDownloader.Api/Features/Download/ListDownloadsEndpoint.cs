using FastEndpoints;

using YtDownloader.Api.Models;
using YtDownloader.Base.Enums;
using YtDownloader.Base.Repositories;

namespace YtDownloader.Api.Features.Download;

public class ListDownloadsEndpoint(IDownloadRepository repository) : Endpoint<EmptyRequest, ICollection<DownloadResponse>>
{
    public IDownloadRepository Repository { get; } = repository;

    public override void Configure()
    {
        Get("/downloads");
        AllowAnonymous();
        Description(x => x.Produces(200).ProducesProblem(500));
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var items = await Repository.Get(DownloadStatus.Pending, DownloadStatus.Downloading, DownloadStatus.Finished, DownloadStatus.Failed, DownloadStatus.Cancelled);
        
        // Exclude videos that are Finished and older than 5 days
        var fiveDaysAgo = DateTime.UtcNow.AddDays(-5);
        var filteredItems = items.Where(x => 
        {
            if (x.Status == DownloadStatus.Finished && x.Finished.HasValue && x.Finished.Value < fiveDaysAgo)
            {
                return false; // Exclude archived (old finished) videos
            }
            return true;
        }).ToList();
        
        await Send.OkAsync([.. filteredItems.Select(x => new DownloadResponse(x))], cancellation: ct);
    }
}