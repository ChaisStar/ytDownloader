
using YtDownloader.Base.Models;

namespace YtDownloader.Core.Services
{
    internal interface IDownloadService
    {
        Task Fail(Download download);
        Task<IReadOnlyList<Download>> GetPendingDownloads();
        Task Start(Download item);
        Task UpdateInfo(Download download);
    }
}