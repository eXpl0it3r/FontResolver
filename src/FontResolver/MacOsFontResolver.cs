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
        var discoveredFonts = new HashSet<string>();

        foreach (var fontDirectory in FontDirectories.Concat(customFontDirectories).Where(Directory.Exists))
        {
            var fontFiles = Directory.GetFiles(fontDirectory, "*.ttf", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(fontDirectory, "*.otf", SearchOption.AllDirectories));

            foreach (var fontFile in fontFiles)
            {
                var extractedFontFamily = FontParser.ExtractFontFamily(fontFile);
                if (extractedFontFamily?.FamilyName != null)
                {
                    discoveredFonts.Add(extractedFontFamily.FamilyName);
                }
                else
                {
                    // Fallback to filename
                    discoveredFonts.Add(Path.GetFileNameWithoutExtension(fontFile));
                }
            }
        }

        return discoveredFonts.ToList();
    }
}