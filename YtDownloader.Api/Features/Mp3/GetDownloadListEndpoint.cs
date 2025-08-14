using FastEndpoints;

using YtDownloader.Api.Models;
using YtDownloader.Core.Services;

namespace YtDownloader.Api.Features.Mp3;

public class GetDownloadListEndpoint(IMp3DownloadService mp3DownloadService) : Endpoint<AddDownloadRequest, ICollection<string>>
{
    public IMp3DownloadService Mp3DownloadService { get; } = mp3DownloadService;

    public override void Configure()
    {
        Get("/mp3/list");
        AllowAnonymous();
        Description(x => x.Produces(200).ProducesProblem(500));
    }

    public override async Task HandleAsync(AddDownloadRequest req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            foreach (var failure in ValidationFailures)
            {
                AddError(failure);
            }
        }
        ThrowIfAnyErrors(400);
        var items = await Mp3DownloadService.GetList(req.Url!);
        await Send.OkAsync([.. items], cancellation: ct);
    }
}