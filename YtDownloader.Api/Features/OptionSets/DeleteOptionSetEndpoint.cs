using FastEndpoints;
using YtDownloader.Base.Repositories;

namespace YtDownloader.Api.Features.OptionSets;

public class DeleteOptionSetEndpoint(IOptionSetRepository repository) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/optionsets/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        await repository.Delete(id);
        await Send.OkAsync(new { }, ct);
    }
}
