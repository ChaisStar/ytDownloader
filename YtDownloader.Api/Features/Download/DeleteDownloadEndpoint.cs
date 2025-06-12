using FastEndpoints;

using YtDownloader.Api.Models;
using YtDownloader.Base.Repositories;

namespace YtDownloader.Api.Features.Download
{
    public class DeleteDownloadEndpoint(IDownloadRepository repository) : Endpoint<DownloadIdRequest, EmptyResponse>
    {
        public override void Configure()
        {
            Delete("/downloads/{id}");
            AllowAnonymous();
            Description(x => x.Produces(200).ProducesProblem(500));
        }

        public override Task HandleAsync(DownloadIdRequest req, CancellationToken ct) => repository.Remove(req.Id);
    }
}