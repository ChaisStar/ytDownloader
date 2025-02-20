
using YtDownloader.Base.Enums;
using YtDownloader.Base.Models;

namespace YtDownloader.Core.Services
{
    internal interface IDownloadService
    {
        Task Fail(Download download);
        Task<IReadOnlyList<Download>> GetPendingDownloads();
        Task<IReadOnlyList<Download>> GetFailedDownloads();
        Task<IReadOnlyList<Download>> GetUndefinedDownloads();
        Task Start(Download item);
        Task UpdateInfo(Download download);
    }
}