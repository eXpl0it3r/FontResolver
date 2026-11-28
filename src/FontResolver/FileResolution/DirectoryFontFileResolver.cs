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
                    EnumerateFontFiles(directory)
                        .Select(f => new FontFile(Path.GetFileNameWithoutExtension(f), f))
                        .ToList()
                );
            }
            catch
            {
                // Silently continue on directory access errors
            }
        }

        return fontInfos;
    }

    private static IEnumerable<string> EnumerateFontFiles(string directory)
    {
        // Font files copied from Windows often have upper case extensions, and one unreadable
        // subfolder mustn't hide the rest of the tree, so the folders are walked one by one
        var files = new List<string>();
        var pending = new Stack<string>();
        pending.Push(directory);

        while (pending.Count > 0)
        {
            var current = pending.Pop();

            try
            {
                files.AddRange(
                    Directory
                        .EnumerateFiles(current)
                        .Where(f =>
                            FontFileResolver.SupportedFontExtensions.Any(e =>
                                f.EndsWith(e, StringComparison.OrdinalIgnoreCase)
                            )
                        )
                );

                foreach (var subdirectory in Directory.EnumerateDirectories(current))
                {
                    // A linked folder can point back up the tree
                    if ((File.GetAttributes(subdirectory) & FileAttributes.ReparsePoint) == 0)
                    {
                        pending.Push(subdirectory);
                    }
                }
            }
            catch (Exception e) when (e is UnauthorizedAccessException or IOException)
            {
                // Skips only this folder
            }
        }

        return files;
    }
}
