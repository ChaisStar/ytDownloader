using System.ComponentModel.DataAnnotations;

namespace YtDownloader.Api.Models;

public class AddDownloadRequest
{
    [Required]
    [Url]
    public string? Url { get; set; }

    public bool Later { get; set; } = false;
}