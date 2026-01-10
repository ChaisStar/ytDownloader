using YtDownloader.Base.Models;

namespace YtDownloader.Base.Repositories;

public interface IOptionSetRepository
{
    Task<IReadOnlyList<DownloadOptionSet>> GetAll();
    Task<IReadOnlyList<DownloadOptionSet>> GetEnabled();
    Task<DownloadOptionSet?> GetById(int id);
    Task Update(DownloadOptionSet optionSet);
    Task<int> Create(DownloadOptionSet optionSet);
    Task Delete(int id);
    Task UpdatePriorities(IEnumerable<(int Id, int Priority)> priorities);
}
