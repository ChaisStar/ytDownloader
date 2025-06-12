using FastEndpoints;

using YtDownloader.Api.Models;

namespace YtDownloader.Api.Features.Cookies
{
    public class SaveCookiesEndpoint() : Endpoint<SaveCookieRequest, EmptyResponse>
    {
        public override void Configure()
        {
            Post("/cookies");
            AllowAnonymous();
            AllowFileUploads();
            Description(x => x.Produces(200).ProducesProblem(500));
        }

        public override async Task HandleAsync(SaveCookieRequest req, CancellationToken ct)
        {
            if (req.Cookies is null)
            {
                ThrowError("No cookies provided", 400);
            }
            string directory = @"/tmp/cookies";
            string cookiesFilePath = Path.Combine(directory, "cookies.txt");

            if (File.Exists(directory))
            {
                Console.WriteLine($"Warning: A file exists at {directory}. Deleting it to create the directory.");
                File.Delete(directory);
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            using var stream = new FileStream(cookiesFilePath, FileMode.Create);
            await req.Cookies.CopyToAsync(stream, ct);
            await stream.FlushAsync(ct);
            await SendOkAsync(new EmptyResponse(), ct);
        }
    }
}