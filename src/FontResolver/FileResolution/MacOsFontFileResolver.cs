namespace FontResolution.FileResolution;

internal static class MacOsFontFileResolver
{
    public static List<FontFile> ResolveFiles()
    {
        var fontDirectories = new List<string>
        {
            "/System/Library/Fonts",
            "/Library/Fonts",
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Library/Fonts"
            ),
        };

        return DirectoryFontFileResolver.ResolveFontFiles(fontDirectories);
    }
}
