using System.Text;
using System.Text.RegularExpressions;

namespace YtDownloader.Core.Extestions;

public static class StringExtensions
{
    public static string SanitizeFileName
        (this string fileName)
    {
        var name = Path.GetFileName(fileName);

        byte[] utf8Bytes = Encoding.UTF8.GetBytes(name);
        name = Encoding.UTF8.GetString(utf8Bytes);

        name = Regex.Replace(name, @"[<>:""/\\|?*\x00-\x1F]", "_");

        name = Regex.Replace(name, @"\s+", "_");
        name = Regex.Replace(name, @"_+", "_");

        if (name.Length > 200) // Залишаємо запас
        {
            var extension = Path.GetExtension(name);
            var baseName = Path.GetFileNameWithoutExtension(name)[..(200 - extension.Length)];
            name = baseName + extension;
        }

        return string.IsNullOrWhiteSpace(name) ? $"default_video_{DateTime.UtcNow.Ticks}.mp4" : name;
    }
}