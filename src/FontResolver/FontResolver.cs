using System.Runtime.InteropServices;
using System.Text;

namespace FontResolver;

public static class FontResolver
{
    private static List<string> CustomFontDirectories { get; } = [];

    public static string? Resolve(string fontName, FontStyle fontStyle)
    {
        var stylizedFontName = StylizeFontName(fontName, fontStyle);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return WindowsFontResolver.Resolve(stylizedFontName);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return LinuxFontResolver.Resolve(stylizedFontName);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return MacOsFontResolver.Resolve(stylizedFontName);
        }

        throw new FontResolverException($"Font Resolver does not support the platform '{RuntimeInformation.OSDescription}'.");
    }

    public static void RegisterCustomFontDirectory(string fontDirectory)
    {
        CustomFontDirectories.Add(fontDirectory);
    }

    public static string StylizeFontName(string fontName, FontStyle fontStyle)
    {
        var stylizedFontName = new StringBuilder(fontName);

        if (fontStyle.Bold)
        {
            stylizedFontName.Append(" Bold");
        }

        if (fontStyle.Italic)
        {
            stylizedFontName.Append(" Italic");
        }

        return stylizedFontName.ToString();
    }

    internal static string? SearchDirectories(string fontName, string[] fontDirectories)
    {
        foreach (var directory in fontDirectories.Concat(CustomFontDirectories).Where(Directory.Exists))
        {
            var fontFiles = Directory.GetFiles(directory, "*.ttf", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(directory, "*.otf", SearchOption.AllDirectories));

            foreach (var fontFile in fontFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(fontFile);
                var normalizedFontName = NormalizeFontFileName(fileName);

                if (normalizedFontName == fontName.ToLowerInvariant())
                {
                    return fontFile;
                }
            }
        }

        return null;
    }

    internal static string NormalizeFontFileName(string fileName)
    {
        var normalized = fileName.ToLowerInvariant()
            .Replace("-", " ")
            .Replace("_", " ");
        
        // Remove common suffixes
        var suffixes = new[] { " (truetype)", " (opentype)", " (type 1)" };
        foreach (var suffix in suffixes)
        {
            if (normalized.EndsWith(suffix))
            {
                normalized = normalized.Substring(0, normalized.Length - suffix.Length).Trim();
                break;
            }
        }

        // Handle common patterns like "Arial-Bold" -> "arial bold"
        // or "DejaVuSans-BoldOblique" -> "dejavusans bold italic"
        normalized = normalized
            .Replace("bolditalic", " bold italic")
            .Replace("boldoblique", " bold italic")
            .Replace("bold", " bold")
            .Replace("italic", " italic")
            .Replace("oblique", " italic")
            .Replace("regular", "")
            .Replace("normal", "");

        while (normalized.Contains("  "))
        {
            normalized = normalized.Replace("  ", " ");
        }

        return normalized.Trim();
    }
}