using System.Diagnostics.CodeAnalysis;
using FontResolution.Parsing;
using Microsoft.Win32;

namespace FontResolution.Discovery;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Platform switching in calling class")]
internal static class WindowsFontDiscovery
{
    public static void DiscoverFonts(FontCache cache)
    {
        try
        {
            using var machineRegistryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", false);
            using var userRegistryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", false);

            foreach (var registryKey in new[] { machineRegistryKey, userRegistryKey })
            {
                if (registryKey == null)
                {
                    continue;
                }

                foreach (var fontName in registryKey.GetValueNames())
                {
                    var fontFilePath = registryKey.GetValue(fontName)?.ToString();
                    
                    DiscoverFont(cache, fontName, fontFilePath);
                }
            }
        }
        catch
        {
            // Silently continue on errors
        }
    }

    private static void DiscoverFont(FontCache cache, string fontName, string? fontFilePath)
    {
        if (fontFilePath == null ||
            !FontDiscoveryService.SupportedFontExtensions.Any(e => fontFilePath.EndsWith(e, StringComparison.InvariantCultureIgnoreCase)))
        {
            return;
        }

        // If the path is not absolute, the font is in the Windows Fonts folder
        if (!Path.IsPathRooted(fontFilePath))
        {
            fontFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), fontFilePath);
        }

        if (!File.Exists(fontFilePath))
        {
            return;
        }

        try
        {
            // Try to extract metadata from the font file
            var metadata = FontParser.ExtractFontMetadata(fontFilePath);
                        
            if (metadata?.FamilyName != null)
            {
                // PreferredFamily name takes priority if available
                if (metadata.PreferredFamily != null && metadata.PreferredFamily != metadata.FamilyName)
                {
                    var stylizedPreferredFamily = FontNameResolver.StylizeFontNamePreferredFamily(metadata);

                    if (cache.GetFont(stylizedPreferredFamily) == null)
                    {
                        cache.SetFont(stylizedPreferredFamily, fontFilePath, metadata);
                    }
                }
                else
                {
                    var stylizedFontNameFamily = FontNameResolver.StylizeFontNameFamily(metadata);
                
                    // Use the extracted family name
                    if (cache.GetFont(stylizedFontNameFamily) == null)
                    {
                        cache.SetFont(stylizedFontNameFamily, fontFilePath, metadata);   
                    }
                }
            }
            else
            {
                // Fallback to cleaning up the registry entry name
                var cleanedName = CleanFontName(fontName);

                if (!string.IsNullOrEmpty(cleanedName))
                {
                    cache.SetFont(cleanedName, fontFilePath, metadata);
                }
            }
        }
        catch
        {
            // Silently skip files that can't be parsed
        }
    }

    private static string CleanFontName(string fontName)
    {
        return fontName.Replace("(TrueType)", string.Empty)
            .Replace("(OpenType)", string.Empty)
            .Replace("(type 1)", string.Empty)
            .Trim();
    }
}