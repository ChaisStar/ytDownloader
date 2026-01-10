using FastEndpoints;
using YtDownloader.Base.Repositories;

namespace YtDownloader.Api.Features.Tags;

public class DeleteTagEndpoint(ITagRepository repository) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/tags/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        await repository.Delete(id);
        await Send.NoContentAsync(ct);
    }
}
