
namespace YtDownloader.Core.Services;

public interface IMp3DownloadService
{
    Task<(string fileName, Stream content)> DownloadMp3(string url);
    Task<IEnumerable<string>> GetList(string url);
}