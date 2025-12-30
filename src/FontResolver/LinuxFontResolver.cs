namespace FontResolver;

internal static class LinuxFontResolver
{
    public static string? Resolve(string fontName)
    {
        var fontDirectories = new[]
        {
            "/usr/share/fonts",
            "/usr/local/share/fonts",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/fonts")
        };

        try
        {
            return FontResolver.SearchDirectories(fontName, fontDirectories) ?? FontResolver.SearchDirectories(fontName.Replace(" ", string.Empty), fontDirectories);
        }
        catch (Exception ex)
        {
            throw new FontResolverException($"Error resolving font '{fontName}' on Linux", ex);
        }
    }
}