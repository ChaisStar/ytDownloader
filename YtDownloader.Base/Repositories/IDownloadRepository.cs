using YtDownloader.Base.Enums;
using YtDownloader.Base.Models;

namespace YtDownloader.Base.Repositories;

public interface IDownloadRepository
{
    Task<Download> Get(int id);

    Task<Download> Get(string url);

    Task<IReadOnlyList<Download>> Get(DateTime from, DateTime? to = null);

    Task<IReadOnlyList<Download>> Get(params DownloadStatus[] downloadStatuses);

    Task<IReadOnlyList<Download>> GetUndefined();

    Task<Download> Create(string url, int? tagId = null);

    Task<Download> Update(Download download, string[] columnsToUpdate);

    Task Remove(int id);
}