using static Microsoft.Win32.Registry;

namespace FontResolution;

internal static class WindowsFontResolver
{
    private const string FontsRegistryKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";

    public static string? Resolve(string fontName)
    {
        var registryFont = SearchRegistry(fontName);

        return registryFont ?? FontResolution.FontResolver.SearchDirectories(fontName, []);
    }

    public static List<string> DiscoverFontFamilies(List<string> customFontDirectories)
    {
        var discoveredFonts = new HashSet<string>();

        try
        {
            using var machineRegistryKey = LocalMachine.OpenSubKey(FontsRegistryKey, false);
            using var userRegistryKey = CurrentUser.OpenSubKey(FontsRegistryKey, false);

            foreach (var registryKey in new[] { machineRegistryKey, userRegistryKey })
            {
                if (registryKey == null)
                {
                    continue;
                }

                foreach (var fontName in registryKey.GetValueNames())
                {
                    var fontFile = registryKey.GetValue(fontName)?.ToString();

                    if (fontFile == null
                        || !(fontFile.EndsWith("ttf", StringComparison.InvariantCultureIgnoreCase) || fontFile.EndsWith("otf", StringComparison.InvariantCultureIgnoreCase)))
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

                    // Try to extract the actual font family name from the file
                    var extractedFontFamily = FontParser.ExtractFontFamily(fontFile);
                    if (extractedFontFamily?.FamilyName != null)
                    {
                        discoveredFonts.Add(extractedFontFamily.FamilyName);
                    }
                    else
                    {
                        // Fallback to cleaning up the registry entry name
                        var cleanedName = fontName.Replace("(TrueType)", string.Empty)
                            .Replace("(OpenType)", string.Empty)
                            .Replace("(type 1)", string.Empty)
                            .Trim();
                        if (!string.IsNullOrEmpty(cleanedName))
                        {
                            discoveredFonts.Add(cleanedName);
                        }
                    }
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
                .Concat(Directory.GetFiles(customFontDirectory, "*.otf", SearchOption.AllDirectories));

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

                    var normalizedFontName = FontResolution.FontResolver.NormalizeFontFileName(valueName);

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
