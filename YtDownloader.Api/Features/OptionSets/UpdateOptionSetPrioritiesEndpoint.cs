using FastEndpoints;
using YtDownloader.Base.Repositories;
using YtDownloader.Api.Models;

namespace YtDownloader.Api.Features.OptionSets;

public class UpdateOptionSetPrioritiesEndpoint(IOptionSetRepository repository) : Endpoint<UpdatePrioritiesRequest>
{
    public override void Configure()
    {
        Post("/optionsets/priorities");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdatePrioritiesRequest req, CancellationToken ct)
    {
        await repository.UpdatePriorities(req.Priorities.Select(p => (p.Id, p.Priority)));
        await Send.OkAsync(new { }, ct);
    }
}
