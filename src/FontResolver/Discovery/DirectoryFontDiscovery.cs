using System.Runtime.InteropServices;
using FontResolution.Parsing;

namespace FontResolution.Discovery;

internal static class DirectoryFontDiscovery
{
    public static void DiscoverFontsFromDirectories(FontCache cache, List<string> customFontDirectories)
    {
        var fontDirectories = new List<string>();

        // Add platform-specific directories
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            fontDirectories.Add(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            fontDirectories.AddRange([
                "/usr/share/fonts",
                "/usr/local/share/fonts",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/fonts")
            ]);
        }

        // Add custom directories
        fontDirectories.AddRange(customFontDirectories);

        DiscoverInDirectories(cache, fontDirectories.ToArray());
    }

    public static void DiscoverInDirectories(FontCache cache, string[] directories)
    {
        foreach (var directory in directories)
        {
            if (!Directory.Exists(directory))
            {
                continue;
            }

            try
            {
                var fontFiles = FontDiscoveryService.SupportedFontExtensions.SelectMany(e => Directory.GetFiles(directory, $"*{e}", SearchOption.AllDirectories));
                
                foreach (var fontFile in fontFiles)
                {
                    try
                    {
                        // Extract metadata from the font file
                        var metadata = FontParser.ExtractFontMetadata(fontFile);
                        
                        if (metadata?.FamilyName != null)
                        {
                            // PreferredFamily name takes priority if available
                            if (metadata.PreferredFamily != null && metadata.PreferredFamily != metadata.FamilyName)
                            {
                                var stylizedPreferredFamily = FontNameResolver.StylizeFontNamePreferredFamily(metadata);

                                if (cache.GetFont(stylizedPreferredFamily) == null)
                                {
                                    cache.SetFont(stylizedPreferredFamily, fontFile, metadata);
                                }
                            }
                            else
                            {
                                var stylizedFontNameFamily = FontNameResolver.StylizeFontNameFamily(metadata);
                            
                                // Use the extracted family name
                                if (cache.GetFont(stylizedFontNameFamily) == null)
                                {
                                    cache.SetFont(stylizedFontNameFamily, fontFile, metadata);
                                }
                            }
                        }
                        else
                        {
                            // Fallback to filename
                            var fileName = Path.GetFileNameWithoutExtension(fontFile);
                            cache.SetFont(fileName, fontFile, metadata);
                        }
                    }
                    catch
                    {
                        // Silently skip files that can't be parsed
                    }
                }
            }
            catch
            {
                // Silently continue on directory access errors
            }
        }
    }
}
