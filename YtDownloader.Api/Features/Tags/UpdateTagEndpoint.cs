using FastEndpoints;
using YtDownloader.Base.Repositories;
using YtDownloader.Api.Models;
using YtDownloader.Base.Models;

namespace YtDownloader.Api.Features.Tags;

public class UpdateTagEndpoint(ITagRepository repository) : Endpoint<TagDto, TagDto>
{
    public override void Configure()
    {
        Put("/tags/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(TagDto req, CancellationToken ct)
    {
        var id = Route<int>("id");
        req.Id = id;
        await repository.Update(Tag.Create(req.Id, req.Name, req.Value, req.Usage, req.Color));
        await Send.OkAsync(req, ct);
    }
}
