using FastEndpoints;

using YtDownloader.Base.Enums;
using YtDownloader.Base.Repositories;

namespace YtDownloader.Api.Features.Download;

public class DeleteArchivedDownloadsEndpoint(IDownloadRepository repository) : EndpointWithoutRequest<DeleteArchivedResponse>
{
    public override void Configure()
    {
        Delete("/archive");
        AllowAnonymous();
        Description(x => x.Produces(200).ProducesProblem(500));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var archivedDownloads = await repository.Get(DownloadStatus.Finished);
            
            // Only delete finished videos older than 5 days
            var fiveDaysAgo = DateTime.UtcNow.AddDays(-5);
            var downloadsToDelete = archivedDownloads
                .Where(x => x.Finished.HasValue && x.Finished.Value < fiveDaysAgo)
                .ToList();

            var deletedCount = 0;
            foreach (var download in downloadsToDelete)
            {
                await repository.Remove(download.Id);
                deletedCount++;
            }

            await Send.OkAsync(new DeleteArchivedResponse 
            { 
                Message = $"Deleted {deletedCount} archived downloads",
                DeletedCount = deletedCount
            }, cancellation: ct);
        }
        catch (Exception ex)
        {
            ThrowError(ex.Message, StatusCodes.Status500InternalServerError);
        }
    }
}
