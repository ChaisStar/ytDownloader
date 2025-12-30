using FastEndpoints;
using System.Diagnostics;

namespace YtDownloader.Api.Features.YtDlp;

public class YtDlpUpdateEndpoint : EndpointWithoutRequest<UpdateResponse>
{
    public override void Configure()
    {
        Post("/ytdlp/update");
        AllowAnonymous();
        Description(x => x.Produces(200).ProducesProblem(500));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/root/.local/bin/pipx",
                    Arguments = "install --upgrade yt-dlp",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                await Send.OkAsync(new UpdateResponse { Message = "yt-dlp updated successfully", Output = output }, cancellation: ct);
            }
            else
            {
                ThrowError(error, StatusCodes.Status500InternalServerError);
            }
        }
        catch (Exception ex)
        {
            ThrowError(ex.Message, StatusCodes.Status500InternalServerError);
        }
    }
}
