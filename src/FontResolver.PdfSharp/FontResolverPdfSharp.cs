using System.Reflection;
using System.Text;
using PdfSharp.Fonts;

namespace FontResolution.PdfSharp;

public class FontResolverPdfSharp : IFontResolver
{
    private Dictionary<string, FontMetadata> _fontCache = new();

    public static string FallbackFont => "Tuffy";
    public FontResolveStrategy ResolveStrategy { get; set; } = FontResolveStrategy.Strict;
    public StringComparison StringComparison { get; set; } = StringComparison.Ordinal;

    public FontResolverInfo ResolveTypeface(string familyName, bool bold, bool italic)
    {
        var fontAttributes = new FontAttributes(
            Style: italic ? FontStyle.Italic : FontStyle.Normal,
            Weight: bold ? FontWeight.Bold : FontWeight.Normal
        );

        var font = FontResolver.Resolve(
            familyName,
            fontAttributes,
            ResolveStrategy,
            StringComparison
        );

        if (font is not null)
        {
            var faceName = CreateFontFace(font);

            if (!_fontCache.ContainsKey(faceName))
            {
                _fontCache[faceName] = font;
            }

            return new FontResolverInfo(faceName);
        }

        // Return fallback if no matching font is found
        var fontFace = CreateFontFace(
            new FontMetadata { PreferredFamily = FallbackFont, Attributes = fontAttributes }
        );

        return new FontResolverInfo(fontFace);
    }

    public byte[]? GetFont(string faceName)
    {
        var cache = Volatile.Read(ref _fontCache);

        if (cache.TryGetValue(faceName, out var font))
        {
            if (File.Exists(font.FilePath))
            {
                return File.ReadAllBytes(font.FilePath);
            }
        }

        if (!faceName.StartsWith(FallbackFont))
        {
            return null;
        }

        // Try to load embedded fallback font
        var assembly = Assembly.GetExecutingAssembly();

        var resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(r => r.EndsWith($"{faceName}.ttf", StringComparison.OrdinalIgnoreCase));
        if (resourceName is null)
        {
            return null;
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return null;
        }

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    public void RegisterCustomFontDirectory(string fontDirectory)
    {
        FontResolver.RegisterCustomFontDirectory(fontDirectory);
        _fontCache.Clear();
    }

    public void ClearCache()
    {
        FontResolver.ClearCache();
        Interlocked.Exchange(ref _fontCache, new Dictionary<string, FontMetadata>());
    }

    public static FontResolverPdfSharp Register()
    {
        var fontResolver = new FontResolverPdfSharp();
        GlobalFontSettings.FontResolver = fontResolver;
        return fontResolver;
    }

    private static string CreateFontFace(FontMetadata font)
    {
        var faceName = new StringBuilder();

        if (!string.IsNullOrEmpty(font.PreferredFamily))
        {
            faceName.Append(font.PreferredFamily);

            if (!string.IsNullOrEmpty(font.PreferredSubfamily))
            {
                faceName.Append($" {font.PreferredSubfamily}");
            }
        }
        else if (!string.IsNullOrEmpty(font.Family))
        {
            faceName.Append(font.Family);
        }

        if (font.Attributes.Weight == FontWeight.Bold)
        {
            faceName.Append(" Bold");
        }

        if (font.Attributes.Style == FontStyle.Italic)
        {
            faceName.Append(" Italic");
        }

        if (
            font.Attributes.Weight == FontWeight.Normal
            && font.Attributes.Style == FontStyle.Normal
        )
        {
            faceName.Append(" Regular");
        }

        return faceName.ToString();
    }
}
