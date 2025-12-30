using System.IO;

namespace FontResolver;

internal static class LinuxFontResolver
{
    private static readonly string[] FontDirectories =
    [
        "/usr/share/fonts",
        "/usr/local/share/fonts",
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/fonts")
    ];

    public static string? Resolve(string fontName)
    {
        try
        {
            return FontResolver.SearchDirectories(fontName, FontDirectories) ?? FontResolver.SearchDirectories(fontName.Replace(" ", string.Empty), FontDirectories);
        }
        catch (Exception ex)
        {
            throw new FontResolverException($"Error resolving font '{fontName}' on Linux", ex);
        }
    }

    public static List<string> DiscoverFontFamilies(List<string> customFontDirectories)
    {
        var discoveredFonts = new List<string>();

        foreach (var fontDirectory in FontDirectories.Concat(customFontDirectories).Where(Directory.Exists))
        {
            var fontFiles = Directory.GetFiles(fontDirectory, "*.ttf", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(fontDirectory, "*.otf", SearchOption.AllDirectories))
                .Select(Path.GetFileNameWithoutExtension);

            discoveredFonts.AddRange(fontFiles);
        }

        return discoveredFonts.Select(f => f.Replace("[wdth,wght]", string.Empty)
                .Replace("[wght]", string.Empty)
                .Replace("-B", string.Empty)
                .Replace("Extra Bold", string.Empty)
                .Replace("ExtB", string.Empty)
                .Replace("Bold", string.Empty)
                .Replace("Semibold", string.Empty)
                .Replace("-BI", string.Empty)
                .Replace("-I", string.Empty)
                .Replace("Italic", string.Empty)
                .Replace("-RI", string.Empty)
                .Replace("-C", string.Empty)
                .Replace("Condensed", string.Empty)
                .Replace("Regular", string.Empty)
                .Replace("-R", string.Empty)
                .Replace("Semilight", string.Empty)
                .Replace("SemiLight", string.Empty)
                .Replace("Light", string.Empty)
                .Replace("-L", string.Empty)
                .Replace("-LI", string.Empty)
                .Replace("Oblique", string.Empty)
                .Replace("Black", string.Empty)
                .Replace("-M", string.Empty)
                .Replace("-MI", string.Empty)
                .Replace("-", string.Empty)
                .Trim())
            .Distinct()
            .ToList();
    }
}