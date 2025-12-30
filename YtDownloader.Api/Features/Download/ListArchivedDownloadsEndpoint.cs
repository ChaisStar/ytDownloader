using FastEndpoints;

using YtDownloader.Api.Models;
using YtDownloader.Base.Enums;
using YtDownloader.Base.Repositories;

namespace YtDownloader.Api.Features.Download;

public class ListArchivedDownloadsEndpoint(IDownloadRepository repository) : Endpoint<EmptyRequest, ICollection<DownloadResponse>>
{
    public IDownloadRepository Repository { get; } = repository;

    public override void Configure()
    {
        Get("/archive");
        AllowAnonymous();
        Description(x => x.Produces(200).ProducesProblem(500));
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var items = await Repository.Get(DownloadStatus.Finished);
        
        // Only return finished videos older than 5 days
        var fiveDaysAgo = DateTime.UtcNow.AddDays(-5);
        var archivedItems = items.Where(x => x.Finished.HasValue && x.Finished.Value < fiveDaysAgo).ToList();
        
        await Send.OkAsync([.. archivedItems.Select(x => new DownloadResponse(x))], cancellation: ct);
    }
}
