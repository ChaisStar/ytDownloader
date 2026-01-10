using YtDownloader.Base.Models;

namespace YtDownloader.Core.Services
{
    public interface IDownloadService
    {
        Task Fail(Download download);
        Task<IReadOnlyList<Download>> GetPendingDownloads();
        Task<IReadOnlyList<Download>> GetFailedDownloads();
        Task<IReadOnlyList<Download>> GetUndefinedDownloads();
        Task Start(Download item);
        Task<Download> Start(string url, int? tagId = null);
        Task UpdateInfo(Download download);
    }
}