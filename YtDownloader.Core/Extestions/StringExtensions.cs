using System.Text;
using System.Text.RegularExpressions;

namespace YtDownloader.Core.Extestions;

public static partial class StringExtensions
{
    public static string SanitizeFileName(this string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return $"default_video_{DateTime.UtcNow.Ticks}.mp4";

        var name = Path.GetFileName(fileName);

        // Забезпечення валідного UTF-8 (опціонально, якщо є проблеми з вхідними даними)
        byte[] utf8Bytes = Encoding.UTF8.GetBytes(name);
        name = Encoding.UTF8.GetString(utf8Bytes);

        var extension = Path.GetExtension(name);
        var baseName = Path.GetFileNameWithoutExtension(name);

        // Нормалізація до NFC для складених символів (вирішення incompatible encoding)
        baseName = baseName.Normalize(NormalizationForm.FormC);

        // Заміна недійсних символів для файлових систем
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            baseName = baseName.Replace(invalidChar, '_');
        }

        // Заміна всіх не-дозволених символів на підкреслення (дозволяємо літери, цифри, _-, пунктуацію, символи)
        baseName = ExtraSymbolsRegex().Replace(baseName, "_");

        // Заміна пробілів і множинних підкреслень
        baseName = WhitespacesRegex().Replace(baseName, "_");
        baseName = CombinedUnderscoresRegex().Replace(baseName, "_");

        // Видалення ведучих/завершальних підкреслень
        baseName = baseName.Trim('_');

        // Обрізаємо, якщо ім’я занадто довге
        const int maxLength = 200;
        if (baseName.Length > maxLength - extension.Length)
        {
            baseName = baseName[..(maxLength - extension.Length)];
        }

        // Перевірка на резервовані імена (Windows)
        string[] reservedNames = { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
        if (Array.Exists(reservedNames, rn => string.Equals(baseName, rn, StringComparison.OrdinalIgnoreCase)))
        {
            baseName = "_" + baseName;
        }

        // Формуємо підсумкове ім’я
        var sanitizedName = baseName + extension;

        // Якщо ім’я порожнє, повертаємо резервне
        return string.IsNullOrWhiteSpace(sanitizedName)
            ? $"default_video_{DateTime.UtcNow.Ticks}.mp4"
            : sanitizedName;
    }

    /// <summary>
    /// Generates a unique filename by appending timestamp if file exists, or progressively shortening name if too long.
    /// </summary>
    public static string GetUniqueFileName(this string fileName, string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return $"default_video_{DateTime.UtcNow.Ticks}.mp4";

        var extension = Path.GetExtension(fileName);
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var fullPath = Path.Combine(directoryPath, fileName);

        // If file doesn't exist, use the original name
        if (!File.Exists(fullPath))
        {
            return fileName;
        }

        // File exists - add timestamp before extension
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var uniqueName = $"{baseName}_{timestamp}{extension}";
        var uniquePath = Path.Combine(directoryPath, uniqueName);

        // If timestamped file also exists (extremely unlikely), progressively shorten base name
        if (File.Exists(uniquePath))
        {
            return ShortenFileNameProgressively(baseName, extension, directoryPath, timestamp);
        }

        return uniqueName;
    }

    /// <summary>
    /// Progressively shortens filename by removing one character at a time from the end of the base name.
    /// Used when standard deduplication isn't enough (e.g., filename already at max length).
    /// </summary>
    private static string ShortenFileNameProgressively(string baseName, string extension, string directoryPath, string timestamp)
    {
        var maxAttempts = baseName.Length;

        for (int i = 1; i <= maxAttempts; i++)
        {
            if (baseName.Length - i <= 0)
                break;

            var shortenedBase = baseName[..(baseName.Length - i)];
            var candidateName = $"{shortenedBase}_{timestamp}{extension}";
            var candidatePath = Path.Combine(directoryPath, candidateName);

            if (!File.Exists(candidatePath))
            {
                return candidateName;
            }
        }

        // Ultimate fallback - use only timestamp
        return $"video_{timestamp}{extension}";
    }

    [GeneratedRegex(@"[^\p{L}\p{N}\p{P}\p{S}_-]")]
    private static partial Regex ExtraSymbolsRegex();
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespacesRegex();
    [GeneratedRegex(@"_+")]
    private static partial Regex CombinedUnderscoresRegex();
}