using System.Diagnostics;

namespace FontResolution.FileResolution;

internal static class LinuxFontFileResolver
{
    public static List<FontFile> ResolveFiles()
    {
        var fontInfos = new List<FontFile>();

        // Starting fc-list throws if fontconfig isn't installed, minimal images often lack `which`
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

            // Unread stderr fills the pipe and blocks fc-list, e.g. with many fontconfig warnings
            process.ErrorDataReceived += (_, _) => { };
            process.BeginErrorReadLine();

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
        var familyPart = parts[1];
        var styleStart = familyPart.IndexOf(':');
        if (styleStart >= 0)
        {
            familyPart = familyPart.Substring(0, styleStart);
        }

        var families = familyPart.Trim().Split(',');

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
