using static Microsoft.Win32.Registry;

namespace FontResolver;

internal static class WindowsFontResolver
{
    public static string? Resolve(string fontName)
    {
        var registryFont = SearchRegistry(fontName);

        return registryFont ?? FontResolver.SearchDirectories(fontName, []);
    }

    private static string? SearchRegistry(string fontName)
    {
        const string fontsRegistryKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";

        try
        {
            using var machineRegistryKey = LocalMachine.OpenSubKey(fontsRegistryKey, false);
            using var userRegistryKey = CurrentUser.OpenSubKey(fontsRegistryKey, false);

            foreach (var registryKey in new[] {machineRegistryKey, userRegistryKey})
            {
                if (registryKey == null)
                {
                    throw new FontResolverException("Unable to open Windows font registry key");
                }

                foreach (var valueName in registryKey.GetValueNames())
                {
                    var fontFile = registryKey.GetValue(valueName)?.ToString();
                    if (string.IsNullOrEmpty(fontFile))
                    {
                        continue;
                    }

                    // If the path is not absolute, the font is in the Windows Fonts folder
                    if (!Path.IsPathRooted(fontFile))
                    {
                        fontFile = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
                            fontFile
                        );
                    }

                    var normalizedFontName = FontResolver.NormalizeFontFileName(valueName);

                    if (normalizedFontName == fontName.ToLowerInvariant())
                    {
                        return fontFile;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new FontResolverException($"Error resolving font '{fontName}' on Windows", ex);
        }

        return null;
    }
}
