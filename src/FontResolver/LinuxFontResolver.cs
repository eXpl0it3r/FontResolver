using System.Diagnostics;
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
            var fontPath = ResolveWithFontConfig(fontName);
            if (fontPath != null)
            {
                return fontPath;
            }
        }
        catch
        {
            // Fallback to directory search
        }

        try
        {
            return FontResolver.SearchDirectories(fontName, FontDirectories);
        }
        catch (Exception ex)
        {
            throw new FontResolverException($"Error resolving font '{fontName}' on Linux", ex);
        }
    }

    public static List<string> DiscoverFontFamilies(List<string> customFontDirectories)
    {
        try
        {
            var fontFamilies = DiscoverFontFamiliesWithFontConfig();
            if (fontFamilies.Count > 0)
            {
                return fontFamilies;
            }
        }
        catch
        {
            // Fallback to directory search
        }

        return DiscoverFontFamiliesWithDirectoryScan(customFontDirectories);
    }

    private static string? ResolveWithFontConfig(string fontName)
    {
        if (!IsFontConfigAvailable())
        {
            return null;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "fc-list",
            Arguments = $"--format=%{{file}} \"{fontName}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            return null;
        }

        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
        {
            var fontPath = output.Trim();
            if (File.Exists(fontPath) && FontResolver.SupportedFontExtensions.Any(ext => fontPath.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase)))
            {
                return fontPath;
            }
        }

        return null;
    }

    private static List<string> DiscoverFontFamiliesWithFontConfig()
    {
        var fontFamilies = new List<string>();

        if (!IsFontConfigAvailable())
        {
            return fontFamilies;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "fc-list",
            Arguments = ": family",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            return fontFamilies;
        }

        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
        {
            var lines = output.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                // fc-list can return multiple families separated by commas
                var families = line.Split(',');
                foreach (var family in families)
                {
                    var trimmedFamily = family.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedFamily))
                    {
                        fontFamilies.Add(trimmedFamily);
                    }
                }
            }
        }

        return fontFamilies.Distinct().OrderBy(f => f).ToList();
    }

    private static bool IsFontConfigAvailable()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "which",
                Arguments = "fc-list",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return false;
            }

            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static List<string> DiscoverFontFamiliesWithDirectoryScan(List<string> customFontDirectories)
    {
        var discoveredFonts = new HashSet<string>();

        foreach (var fontDirectory in FontDirectories.Concat(customFontDirectories).Where(Directory.Exists))
        {
            var fontFiles = new List<string>();
            foreach (var extension in FontResolver.SupportedFontExtensions)
            {
                fontFiles.AddRange(Directory.GetFiles(fontDirectory, $"*{extension}", SearchOption.AllDirectories));
            }

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