using FastEndpoints;

namespace YtDownloader.Api.Features.Cookies;

public class CookiesInfoEndpoint : EndpointWithoutRequest<CookiesInfo>
{
    private const string CookiesPath = "/tmp/cookies/cookies.txt";

    public override void Configure()
    {
        Get("/cookies/info");
        AllowAnonymous();
        Description(x => x.Produces(200).ProducesProblem(500));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Task.CompletedTask;

        try
        {
            if (!File.Exists(CookiesPath))
            {
                await Send.OkAsync(new CookiesInfo { Exists = false, Size = 0 }, cancellation: ct);
                return;
            }

            var fileInfo = new FileInfo(CookiesPath);
            await Send.OkAsync(new CookiesInfo
            {
                LastModified = fileInfo.LastWriteTimeUtc,
                Size = fileInfo.Length,
                Exists = true
            }, cancellation: ct);
        }
        catch (Exception ex)
        {
            ThrowError(ex.Message, StatusCodes.Status500InternalServerError);
        }
    }
}
