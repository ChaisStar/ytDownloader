using FastEndpoints;
using YtDownloader.Base.Repositories;
using YtDownloader.Api.Models;
using YtDownloader.Base.Models;

namespace YtDownloader.Api.Features.OptionSets;

public class CreateOptionSetEndpoint(IOptionSetRepository repository) : Endpoint<OptionSetDto, int>
{
    public override void Configure()
    {
        Post("/optionsets");
        AllowAnonymous();
    }

    public override async Task HandleAsync(OptionSetDto req, CancellationToken ct)
    {
        var model = new DownloadOptionSet
        {
            Name = req.Name,
            Format = req.Format,
            MergeOutputFormat = req.MergeOutputFormat,
            EmbedThumbnail = req.EmbedThumbnail,
            IsEnabled = req.IsEnabled,
            Priority = req.Priority,
            ExtractAudio = req.ExtractAudio,
            AudioFormat = req.AudioFormat
        };

        var id = await repository.Create(model);
        await Send.OkAsync(id, ct);
    }
}
