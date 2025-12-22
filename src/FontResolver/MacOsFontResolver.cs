namespace FontResolver;

internal static class MacOsFontResolver
{
    public static string? Resolve(string fontName)
    {
        var fontDirectories = new[]
        {
            "/System/Library/Fonts",
            "/Library/Fonts",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Fonts")
        };

        try
        {
            return FontResolver.SearchDirectories(fontName, fontDirectories);
        }
        catch (Exception ex)
        {
            throw new FontResolverException($"Error resolving font '{fontName}' on macOS", ex);
        }
    }
}