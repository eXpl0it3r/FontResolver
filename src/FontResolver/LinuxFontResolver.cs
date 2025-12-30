using System.Diagnostics;

namespace FontResolver;

internal static class LinuxFontResolver
{
    public static string? Resolve(string fontName)
    {
        var fontConfigFontPath = SearchFontConfig(fontName);
        if (fontConfigFontPath != null)
        {
            throw new Exception($"{fontName}: {fontConfigFontPath}");
            return fontConfigFontPath;
        }

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

    internal static string? SearchFontConfig(string fontName)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "fc-list",
                Arguments = $"--format=\"%{{file}}\\n\" \"{fontName}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                return null;
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
            {
                return null;
            }

            var fontPath = output.Split('\n').FirstOrDefault()?.Trim();

            if (fontPath != null && File.Exists(fontPath))
            {
                return fontPath;
            }

            return null;
        }
        catch
        {
            // If fontconfig is not available or any error occurs, return null
            // to fall back to directory searching
            return null;
        }
    }
}