using FastEndpoints;

using YtDownloader.Api.Models;
using YtDownloader.Base.Repositories;

namespace YtDownloader.Api.Features.Download;

public class GetDownloadEndpoint(IDownloadRepository repository) : Endpoint<DownloadIdRequest, DownloadResponse>
{
    public IDownloadRepository Repository { get; } = repository;

    public override void Configure()
    {
        Get("/downloads/{id}");
        AllowAnonymous();
        Description(x => x.Produces(200).ProducesProblem(500));
    }

    public override async Task HandleAsync(DownloadIdRequest req, CancellationToken ct) =>
        await SendAsync(new DownloadResponse(await Repository.Get(req.Id)), cancellation: ct);
}