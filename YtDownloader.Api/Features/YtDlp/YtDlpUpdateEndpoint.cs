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
                    FileName = "/bin/sh",
                    Arguments = "-c \"pipx install --upgrade yt-dlp\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync(ct);

            if (process.ExitCode == 0)
            {
                await Send.OkAsync(new UpdateResponse { Message = "yt-dlp updated successfully", Output = output }, cancellation: ct);
            }
            else
            {
                ThrowError($"Failed to update yt-dlp: {error}", StatusCodes.Status500InternalServerError);
            }
        }
        catch (Exception ex)
        {
            ThrowError($"Error updating yt-dlp: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}
