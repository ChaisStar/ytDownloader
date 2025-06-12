using FastEndpoints;

using FluentValidation.Results;

using YtDownloader.Api.Models;
using YtDownloader.Base.Repositories;

namespace YtDownloader.Api.Features.Download;

public class CreateDownloadEndpoint(IDownloadRepository repository) : Endpoint<AddDownloadRequest, DownloadResponse>
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
        var item = await repository.Create(req.Url!, req.Later);
        await SendAsync(new DownloadResponse(item), cancellation: ct);
    }
}