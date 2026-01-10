using FastEndpoints;
using YtDownloader.Base.Repositories;
using YtDownloader.Api.Models;

namespace YtDownloader.Api.Features.Tags;

public class ListTagsEndpoint(ITagRepository repository) : EndpointWithoutRequest<List<TagDto>>
{
    public override void Configure()
    {
        Get("/tags");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var tags = await repository.GetAll();
        await Send.OkAsync(tags.Select(t => new TagDto
        {
            Id = t.Id,
            Name = t.Name,
            Value = t.Value,
            Usage = t.Usage,
            Color = t.Color
        }).ToList(), ct);
    }
}
