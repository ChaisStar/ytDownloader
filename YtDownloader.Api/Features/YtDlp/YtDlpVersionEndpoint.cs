using FastEndpoints;

using FluentValidation.Results;

using YtDownloader.Api.Models;
using YtDownloader.Core.Services;

namespace YtDownloader.Api.Features.YtDlp;

public class YtDlpVersionEndpoint() : EndpointWithoutRequest<string>
{
    public IYtDlpService YtDlpService { get; } = null!;
    public override void Configure()
    {
        Get("/ytdlp/version");
        AllowAnonymous();
        Description(x => x.Produces(200).ProducesProblem(500));
    }
    public override async Task HandleAsync(CancellationToken ct)
    {
        var version = await YtDlpService.GetVersion();
        await Send.OkAsync(version, cancellation: ct);
    }
}

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