using System.IO;
using static Microsoft.Win32.Registry;

namespace FontResolver;

internal static class WindowsFontResolver
{
    private const string FontsRegistryKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";

    public static string? Resolve(string fontName)
    {
        var registryFont = SearchRegistry(fontName);

        return registryFont ?? FontResolver.SearchDirectories(fontName, []);
    }

    public static List<string> DiscoverFontFamilies(List<string> customFontDirectories)
    {
        var discoveredFonts = new List<string>();

        try
        {
            using var machineRegistryKey = LocalMachine.OpenSubKey(FontsRegistryKey, false);
            using var userRegistryKey = CurrentUser.OpenSubKey(FontsRegistryKey, false);

            foreach (var registryKey in new[] { machineRegistryKey, userRegistryKey })
            {
                if (registryKey == null)
                {
                    throw new FontResolverException("Unable to open Windows font registry key");
                }

                foreach (var fontName in registryKey.GetValueNames())
                {
                    var fontFile = registryKey.GetValue(fontName)?.ToString();

                    if (fontFile == null
                        || !(fontFile.EndsWith("ttf", StringComparison.InvariantCultureIgnoreCase) || fontFile.EndsWith("otf", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }

                    discoveredFonts.Add(fontName);
                }
            }
        }
        catch (Exception)
        {
            // Ignore errors during font discovery
        }

        foreach (var customFontDirectory in customFontDirectories.Where(Directory.Exists))
        {
            var fontFiles = Directory.GetFiles(customFontDirectory, "*.ttf", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(customFontDirectory, "*.otf", SearchOption.AllDirectories))
                .Select(Path.GetFileNameWithoutExtension);

            discoveredFonts.AddRange(fontFiles);
        }

        return discoveredFonts.Select(f => f.Replace("(TrueType)", string.Empty)
            .Replace("(OpenType)", string.Empty)
            .Replace("(type 1)", string.Empty)
            .Replace("Extra Bold", string.Empty)
            .Replace("ExtB", string.Empty)
            .Replace("Bold", string.Empty)
            .Replace("Italic", string.Empty)
            .Replace("Condensed", string.Empty)
            .Replace("Regular", string.Empty)
            .Replace("Semilight", string.Empty)
            .Replace("SemiLight", string.Empty)
            .Replace("Light", string.Empty)
            .Replace("Oblique", string.Empty)
            .Replace("Black", string.Empty)
            .Replace("Semibold", string.Empty)
            .Trim())
            .Distinct()
            .ToList();
    }

    private static string? SearchRegistry(string fontName)
    {
        try
        {
            using var machineRegistryKey = LocalMachine.OpenSubKey(FontsRegistryKey, false);
            using var userRegistryKey = CurrentUser.OpenSubKey(FontsRegistryKey, false);

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
