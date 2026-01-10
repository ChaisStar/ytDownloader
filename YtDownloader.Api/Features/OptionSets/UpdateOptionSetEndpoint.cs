using FastEndpoints;
using YtDownloader.Base.Repositories;
using YtDownloader.Api.Models;
using YtDownloader.Base.Models;

namespace YtDownloader.Api.Features.OptionSets;

public class UpdateOptionSetEndpoint(IOptionSetRepository repository) : Endpoint<OptionSetDto>
{
    public override void Configure()
    {
        Put("/optionsets/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(OptionSetDto req, CancellationToken ct)
    {
        var id = Route<int>("id");
        var existing = await repository.GetById(id);
        if (existing == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        existing.Name = req.Name;
        existing.Format = req.Format;
        existing.MergeOutputFormat = req.MergeOutputFormat;
        existing.EmbedThumbnail = req.EmbedThumbnail;
        existing.IsEnabled = req.IsEnabled;
        existing.Priority = req.Priority;
        existing.ExtractAudio = req.ExtractAudio;
        existing.AudioFormat = req.AudioFormat;

        await repository.Update(existing);
        await Send.OkAsync(new { }, ct);
    }
}
