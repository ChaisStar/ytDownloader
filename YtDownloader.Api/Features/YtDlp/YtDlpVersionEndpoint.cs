using FastEndpoints;

using YtDownloader.Core.Services;

namespace YtDownloader.Api.Features.YtDlp;

public class YtDlpVersionEndpoint(IYtDlService ytDlService) : EndpointWithoutRequest<string>
{
    public override void Configure()
    {
        Get("/ytdlp/version");
        AllowAnonymous();
        Description(x => x.Produces(200).ProducesProblem(500));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var version = await ytDlService.GetVersion();
        await Send.OkAsync(version, cancellation: ct);
    }
}