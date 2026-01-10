using FastEndpoints;
using YtDownloader.Base.Repositories;
using YtDownloader.Api.Models;

namespace YtDownloader.Api.Features.Tags;

public class CreateTagEndpoint(ITagRepository repository) : Endpoint<TagDto, TagDto>
{
    public override void Configure()
    {
        Post("/tags");
        AllowAnonymous();
    }

    public override async Task HandleAsync(TagDto req, CancellationToken ct)
    {
        var tag = await repository.Create(req.Name, req.Value, req.Usage, req.Color);
        await Send.OkAsync(new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Value = tag.Value,
            Usage = tag.Usage,
            Color = tag.Color
        }, ct);
    }
}
