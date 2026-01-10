using FastEndpoints;
using YtDownloader.Base.Repositories;
using YtDownloader.Api.Models;

namespace YtDownloader.Api.Features.OptionSets;

public class ListOptionSetsEndpoint(IOptionSetRepository repository) : EndpointWithoutRequest<List<OptionSetDto>>
{
    public override void Configure()
    {
        Get("/optionsets");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var sets = await repository.GetAll();
        await Send.OkAsync(sets.Select(s => new OptionSetDto
        {
            Id = s.Id,
            Name = s.Name,
            Format = s.Format,
            MergeOutputFormat = s.MergeOutputFormat,
            EmbedThumbnail = s.EmbedThumbnail,
            IsEnabled = s.IsEnabled,
            Priority = s.Priority,
            ExtractAudio = s.ExtractAudio,
            AudioFormat = s.AudioFormat
        }).ToList(), ct);
    }
}
