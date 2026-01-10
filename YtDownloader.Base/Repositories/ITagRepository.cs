using YtDownloader.Base.Enums;
using YtDownloader.Base.Models;

namespace YtDownloader.Base.Repositories;

public interface ITagRepository
{
    Task<IReadOnlyList<Tag>> GetAll();
    Task<Tag?> GetById(int id);
    Task<Tag> Create(string name, string value, TagUsage usage, string? color);
    Task Update(Tag tag);
    Task Delete(int id);
}
