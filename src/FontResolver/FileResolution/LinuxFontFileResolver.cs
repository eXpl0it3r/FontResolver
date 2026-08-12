using System.Diagnostics;

namespace FontResolution.FileResolution;

internal static class LinuxFontFileResolver
{
    public static List<FontFile> ResolveFiles()
    {
        var fontInfos = new List<FontFile>();

        if (!IsFontConfigAvailable())
        {
            return fontInfos;
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
                CreateNoWindow = true,
            };

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return fontInfos;
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
            {
                return fontInfos;
            }

            var lines = output.Split(['\n'], StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var fontFamilies = ResolveFile(line);

                if (fontFamilies.Count > 0)
                {
                    fontInfos.AddRange(fontFamilies);
                }
            }
        }
        catch
        {
            // Silently continue on errors
        }

        return fontInfos;
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
                CreateNoWindow = true,
            };

            using var process = Process.Start(startInfo);
            if (process is null)
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

    private static List<FontFile> ResolveFile(string line)
    {
        var fonts = new List<FontFile>();

        // Parse fc-list output: "path: family1,family2:style=style"
        var parts = line.Split([':'], 2, StringSplitOptions.None);
        if (parts.Length < 2)
        {
            return fonts;
        }

        var fontFilePath = parts[0].Trim();
        var families = parts[1].Trim().Split(',');

        if (
            !File.Exists(fontFilePath)
            || !FontFileResolver.SupportedFontExtensions.Any(e =>
                fontFilePath.EndsWith(e, StringComparison.InvariantCultureIgnoreCase)
            )
        )
        {
            return fonts;
        }

        foreach (var family in families)
        {
            var familyName = family.Trim();
            if (!string.IsNullOrEmpty(familyName) && File.Exists(fontFilePath))
            {
                fonts.Add(new FontFile(familyName, fontFilePath));
            }
        }
        return fonts;
    }
}
