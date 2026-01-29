using System.Diagnostics;
using FontResolution.Parsing;

namespace FontResolution.Discovery;

internal static class LinuxFontDiscovery
{
    public static void DiscoverFonts(FontCache cache)
    {
        if (!IsFontConfigAvailable())
        {
            return;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "fc-list",
                Arguments = ":",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return;
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
            {
                return;
            }

            // Parse fc-list output: "path: family-list"
            var lines = output.Split(['\n'], StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                DiscoverFont(cache, line);
            }
        }
        catch
        {
            // Silently continue on errors
        }
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

    private static void DiscoverFont(FontCache cache, string line)
    {
        var parts = line.Split([':'], 2, StringSplitOptions.None);
        if (parts.Length < 2)
        {
            return;
        }

        var fontPath = parts[0].Trim();
        var families = parts[1].Trim().Split(',');

        if (!File.Exists(fontPath) || 
            !FontDiscoveryService.SupportedFontExtensions.Any(e => fontPath.EndsWith(e, StringComparison.InvariantCultureIgnoreCase)))
        {
            return;
        }

        // Cache each family name
        foreach (var family in families)
        {
            var familyName = family.Trim();
            if (!string.IsNullOrEmpty(familyName))
            {
                CacheFontIfValid(cache, familyName, fontPath);
            }
        }
    }

    private static void CacheFontIfValid(FontCache cache, string fontName, string fontFile)
    {
        // Check if already cached with a different path
        var cached = cache.GetFont(fontName);
        if (cached != null)
        {
            return;
        }

        if (!File.Exists(fontFile))
        {
            return;
        }

        try
        {
            // Try to extract metadata
            var metadata = FontParser.ExtractFontMetadata(fontFile);
            cache.SetFont(fontName, fontFile, metadata);
        }
        catch
        {
            // Silently skip if we can't parse the font
        }
    }
}