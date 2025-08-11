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

        // Видалення емодзі, якщо не потрібні (опціонально)
        // baseName = Regex.Replace(baseName, @"[\p{So}]", "_");

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

    [GeneratedRegex(@"[^\p{L}\p{N}\p{P}\p{S}_-]")]
    private static partial Regex ExtraSymbolsRegex();
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespacesRegex();
    [GeneratedRegex(@"_+")]
    private static partial Regex CombinedUnderscoresRegex();
}