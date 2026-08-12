using System.Runtime.InteropServices;

namespace FontResolution.FileResolution;

internal class FontFileResolver
{
    private readonly List<string> _customFontDirectories = [];

    public static string[] SupportedFontExtensions { get; } = [".ttf", ".otf"];

    public IReadOnlyList<FontFile> ResolveFiles()
    {
        var fontInfos = new List<FontFile>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            fontInfos = WindowsFontFileResolver.ResolveFiles();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            fontInfos = LinuxFontFileResolver.ResolveFiles();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            fontInfos = MacOsFontFileResolver.ResolveFiles();
        }

        // Always use directory scan as fallback
        fontInfos.AddRange(DirectoryFontFileResolver.ResolveFiles(_customFontDirectories));

        return fontInfos;
    }

    public void RegisterCustomFontDirectory(string fontDirectory)
    {
        if (_customFontDirectories.Contains(fontDirectory))
        {
            return;
        }

        _customFontDirectories.Add(fontDirectory);
    }
}
