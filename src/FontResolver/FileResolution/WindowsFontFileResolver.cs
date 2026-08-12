using System.Diagnostics.CodeAnalysis;
using Microsoft.Win32;

namespace FontResolution.FileResolution;

[SuppressMessage(
    "Interoperability",
    "CA1416:Validate platform compatibility",
    Justification = "Platform switching in calling class"
)]
internal static class WindowsFontFileResolver
{
    public static List<FontFile> ResolveFiles()
    {
        var fontInfos = new List<FontFile>();
        try
        {
            using var machineRegistryKey = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts",
                false
            );
            using var userRegistryKey = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts",
                false
            );
            foreach (var registryKey in new[] { machineRegistryKey, userRegistryKey })
            {
                if (registryKey is null)
                {
                    continue;
                }

                foreach (var fontName in registryKey.GetValueNames())
                {
                    var fontFilePath = registryKey.GetValue(fontName)?.ToString();

                    if (fontFilePath is null)
                    {
                        continue;
                    }

                    var fontFile = ResolveFontFile(fontFilePath);

                    if (fontFile is not null)
                    {
                        fontInfos.Add(new FontFile(fontName, fontFile));
                    }
                }
            }
        }
        catch
        {
            // Silently continue on errors
        }

        return fontInfos;
    }

    private static string? ResolveFontFile(string fontFilePath)
    {
        if (
            !FontFileResolver.SupportedFontExtensions.Any(e =>
                fontFilePath.EndsWith(e, StringComparison.InvariantCultureIgnoreCase)
            )
        )
        {
            return null;
        }

        // If the path is not absolute, the font is in the Windows Fonts folder
        if (!Path.IsPathRooted(fontFilePath))
        {
            fontFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
                fontFilePath
            );
        }

        if (!File.Exists(fontFilePath))
        {
            return null;
        }

        return fontFilePath;
    }
}
