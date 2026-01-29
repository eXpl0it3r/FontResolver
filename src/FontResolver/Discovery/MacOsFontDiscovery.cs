namespace FontResolution.Discovery;

internal static class MacOsFontDiscovery
{
    public static void DiscoverFonts(FontCache cache)
    {
        var fontDirectories = new[]
        {
            "/System/Library/Fonts",
            "/Library/Fonts",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Fonts")
        };

        DirectoryFontDiscovery.DiscoverInDirectories(cache, fontDirectories);
    }
}