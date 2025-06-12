using FastEndpoints;

using YtDownloader.Api.Models;
using YtDownloader.Core.Services;

namespace YtDownloader.Api.Features.Mp3;

public class AddMp3DownloadEndpoint(IMp3DownloadService mp3DownloadService) : Endpoint<AddDownloadRequest, EmptyResponse>
{
    public IMp3DownloadService Mp3DownloadService { get; } = mp3DownloadService;

    public override void Configure()
    {
        Get("/mp3");
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
        var (fileName, content) = await Mp3DownloadService.DownloadMp3(req.Url!);
        await SendStreamAsync(content, fileName, contentType: "audio/mpeg", cancellation: ct);
    }
}