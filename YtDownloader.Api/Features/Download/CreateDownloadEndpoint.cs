using FastEndpoints;
using FluentValidation.Results;
using YtDownloader.Api.Models;
using YtDownloader.Core.Services;

namespace YtDownloader.Api.Features.Download;

public class CreateDownloadEndpoint(IDownloadService downloadService) : Endpoint<AddDownloadRequest, DownloadResponse>
{
    public override void Configure()
    {
        Post("/downloads");
        AllowAnonymous();
        Description(x => x.Produces(200).ProducesProblem(500));
    }

    public override async Task HandleAsync(AddDownloadRequest req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            foreach (ValidationFailure failure in ValidationFailures)
            {
                AddError(failure);
            }
        }
        ThrowIfAnyErrors(400);
        
        var item = await downloadService.Start(req.Url!, req.TagId);
        await Send.OkAsync(new DownloadResponse(item), cancellation: ct);
    }
}