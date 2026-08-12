using System.Runtime.InteropServices;

namespace FontResolution.FileResolution;

internal static class DirectoryFontFileResolver
{
    public static List<FontFile> ResolveFiles(List<string> customFontDirectories)
    {
        var fontDirectories = new List<string>();

        // Add platform-specific directories
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            fontDirectories.Add(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            fontDirectories.AddRange([
                "/usr/share/fonts",
                "/usr/local/share/fonts",
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".fonts"
                ),
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".local/share/fonts"
                ),
            ]);
        }

        // Add custom directories
        fontDirectories.AddRange(customFontDirectories);

        return ResolveFontFiles(fontDirectories);
    }

    public static List<FontFile> ResolveFontFiles(List<string> directories)
    {
        var fontInfos = new List<FontFile>();

        foreach (var directory in directories)
        {
            if (!Directory.Exists(directory))
            {
                continue;
            }

            try
            {
                fontInfos.AddRange(
                    FontFileResolver
                        .SupportedFontExtensions.SelectMany(e =>
                            Directory.GetFiles(directory, $"*{e}", SearchOption.AllDirectories)
                        )
                        .Select(f => new FontFile(Path.GetFileNameWithoutExtension(f), f))
                );
            }
            catch
            {
                // Silently continue on directory access errors
            }
        }

        return fontInfos;
    }
}
