using System.Reflection;
using PdfSharp.Fonts;

namespace FontResolution.PdfSharp;

public class FontResolverPdfSharp : IFontResolver
{
    // Using a cache to bridge the two-phased resolving calls by PDFSharp
    private Dictionary<string, string> FontPathCache { get; } = new();
    
    public static string FallbackFont => "Tuffy";

    public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic)
    {
        var style = new FontAttributes(bold ? FontWeight.Bold : FontWeight.Normal, italic ? FontStyle.Italic : FontStyle.Normal);
        var stylizedFontName = FontNameResolver.StylizeFontNameStrict(familyName, style);
        
        if (FontPathCache.ContainsKey(stylizedFontName))
        {
            return new FontResolverInfo(stylizedFontName);
        }
        
        var fontPath = FontResolver.Resolve(familyName, style);
        
        if (fontPath == null)
        {
            return new FontResolverInfo(FontNameResolver.StylizeFontNameStrict(FallbackFont, style));
        }
        
        FontPathCache[stylizedFontName] = fontPath;

        return new FontResolverInfo(stylizedFontName);
    }

    public byte[]? GetFont(string faceName)
    {
        if (FontPathCache.TryGetValue(faceName, out var fontPath))
        {
            if (File.Exists(fontPath))
            {
                return File.ReadAllBytes(fontPath);
            }
        }

        if (!faceName.StartsWith(FallbackFont))
        {
            return null;
        }

        // Try to load embedded fallback font
        var assembly = Assembly.GetExecutingAssembly();
        
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(r => r.EndsWith($"{faceName}.ttf", StringComparison.OrdinalIgnoreCase));
        if (resourceName == null)
        {
            return null;
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            return null;
        }

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    public static void Register()
    {
        GlobalFontSettings.FontResolver = new FontResolverPdfSharp();
    }

    public static void RegisterCustomFontDirectory(string fontDirectory)
    {
        FontResolver.RegisterCustomFontDirectory(fontDirectory);
    }
}