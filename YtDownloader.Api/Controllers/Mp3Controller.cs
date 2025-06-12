using Microsoft.AspNetCore.Mvc;
using YtDownloader.Api.Models;
using YtDownloader.Core.Services;

namespace YtDownloader.Api.Controllers;

[ApiController]
[Route("mp3")]
public class Mp3Controller(IMp3DownloadService mp3DownloadService) : ControllerBase
{
    [HttpPost]
    [Route("list")]
    public async Task<IActionResult> GetListAsync(AddDownloadRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var item = await mp3DownloadService.GetList(request.Url!);
        return Ok(item);
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> AddDownloadAsync(AddDownloadRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (fileName, content) = await mp3DownloadService.DownloadMp3(request.Url!);
        return File(content, "audio/mpeg", fileName);
    }
}