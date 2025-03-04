namespace YtDownloader.Core.Services;

class Mp3DownloadService(IYtDlService ytDlService) : IMp3DownloadService
{
    public async Task<IEnumerable<string>> GetList(string url)
    {
        var result = await ytDlService.GetVideoData(url);
        return result.Data.Entries?.Select(e => e.Url) ?? [url] ;
    }

    public async Task<(string fileName, Stream content)> DownloadMp3(string url)
    {
        var result = await ytDlService.RunMp3PlaylistDownload(url);
        var memoryStream = new MemoryStream();
        await using (var fileStream = new FileStream(result.Data, FileMode.Open, FileAccess.Read))
        {
            await fileStream.CopyToAsync(memoryStream);
        }
        File.Delete(result.Data);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return (result.Data.Replace("/tmp/", ""), memoryStream);
    }
}
