using System.Reflection;
using PdfSharp.Fonts;

namespace FontResolver.PdfSharp;

public class FontResolverPdfSharp : IFontResolver
{
    private Dictionary<string, string> FontPathCache { get; } = new();

    public static string FallbackFont => "Tuffy";

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        var style = new FontStyle(isBold, isItalic);
        var stylizedFontName = FontResolver.StylizeFontName(familyName, style);

        if (FontPathCache.ContainsKey(stylizedFontName))
        {
            return new FontResolverInfo(stylizedFontName);
        }

        var fontPath = FontResolver.Resolve(familyName, style);

        if (fontPath == null)
        {
            return new FontResolverInfo(FontResolver.StylizeFontName(FallbackFont, style));
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