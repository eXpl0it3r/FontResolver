namespace FontResolver;

internal static class MacOsFontResolver
{
    private static readonly string[] FontDirectories =
    [
        "/System/Library/Fonts",
        "/Library/Fonts",
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Fonts")
    ];

    public static string? Resolve(string fontName)
    {
        try
        {
            return FontResolver.SearchDirectories(fontName, FontDirectories);
        }
        catch (Exception ex)
        {
            throw new FontResolverException($"Error resolving font '{fontName}' on macOS", ex);
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

        return discoveredFonts.Select(f => f.Replace("Extra Bold", string.Empty)
                .Replace("ExtB", string.Empty)
                .Replace("Bold", string.Empty)
                .Replace("Bol", string.Empty)
                .Replace("Semibold", string.Empty)
                .Replace("Italic", string.Empty)
                .Replace("Ita", string.Empty)
                .Replace("Condensed", string.Empty)
                .Replace("Regular", string.Empty)
                .Replace("Reg", string.Empty)
                .Replace("Semilight", string.Empty)
                .Replace("SemiLight", string.Empty)
                .Replace("Light", string.Empty)
                .Replace("Oblique", string.Empty)
                .Replace("Black", string.Empty)
                .Replace("-", string.Empty)
                .Trim())
            .Distinct()
            .ToList();
    }
}