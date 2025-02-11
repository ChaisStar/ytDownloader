using Microsoft.AspNetCore.Mvc;

using YtDownloader.Api.Models;
using YtDownloader.Base.Enums;
using YtDownloader.Base.Repositories;

namespace YtDownloader.Api.Controllers;

[ApiController]
[Route("downloads")]
public class DownloadController(IDownloadRepository repository) : ControllerBase
{
    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetListAsync()
    {
        var items = await repository.Get(DownloadStatus.Pending, DownloadStatus.Downloading, DownloadStatus.Finished, DownloadStatus.Failed, DownloadStatus.Cancelled);
        return Ok(items.Select(x => new DownloadResponse(x)));
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> GetAsync(int id)
    {
        var item = await repository.Get(id);
        return Ok(new DownloadResponse(item));
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> AddDownloadAsync(AddDownloadRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var item = await repository.Create(request.Url!, request.Later);
        return Ok(new DownloadResponse(item));
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        await repository.Remove(id);
        return Ok();
    }
}