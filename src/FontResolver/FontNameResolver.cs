using System.Text;

namespace FontResolution;

public static class FontNameResolver
{
    public static string StylizeFontNameFamily(FontMetadata metadata)
    {
        if (metadata.FamilyName is null || metadata.Attributes is null)
        {
            return string.Empty;
        }
        
        return BuildStylizedFontName(metadata.FamilyName, MatchWeightStrict(metadata.Attributes.Weight), MatchStyleStrict(metadata.Attributes.Style));
    }
    
    public static string StylizeFontNamePreferredFamily(FontMetadata metadata)
    {
        if (metadata.PreferredFamily is null || metadata.PreferredSubfamily is null)
        {
            return string.Empty;
        }
        
        return string.IsNullOrEmpty(metadata.PreferredSubfamily)
            ? metadata.PreferredFamily
            : $"{metadata.PreferredFamily} {metadata.PreferredSubfamily}";
    }
    
    public static string StylizeFontNameStrict(string fontName, FontAttributes fontAttributes)
    {
        return BuildStylizedFontName(fontName, MatchWeightStrict(fontAttributes.Weight), MatchStyleStrict(fontAttributes.Style));
    }

    public static string[] StylizeFontNameClosest(string fontName, FontAttributes fontAttributes)
    {
        var fontNames = new List<string>
        {
            StylizeFontNameStrict(fontName, fontAttributes),
            BuildStylizedFontName(fontName, MatchWeightStrict(fontAttributes.Weight), MatchStyleObliqueAndItalic(fontAttributes.Style)),
            BuildStylizedFontName(fontName, MatchWeightMiddle(fontAttributes.Weight), MatchStyleStrict(fontAttributes.Style)),
            BuildStylizedFontName(fontName, MatchWeightMiddle(fontAttributes.Weight), MatchStyleObliqueAndItalic(fontAttributes.Style)),
            BuildStylizedFontName(fontName, MatchWeightOutward(fontAttributes.Weight), MatchStyleStrict(fontAttributes.Style)),
            BuildStylizedFontName(fontName, MatchWeightOutward(fontAttributes.Weight), MatchStyleObliqueAndItalic(fontAttributes.Style)),
            fontName
        };

        return fontNames.Distinct().Where(x => !string.IsNullOrEmpty(x)).ToArray();
    }
    
    private static string MatchStyleStrict(FontStyle fontStyle)
    {
        return fontStyle switch
        {
            FontStyle.Normal => string.Empty,
            _ => fontStyle.ToString()
        };
    }
    
    private static string MatchStyleObliqueAndItalic(FontStyle fontStyle)
    {
        return fontStyle switch
        {
            FontStyle.Oblique or FontStyle.Italic => nameof(FontStyle.Italic),
            _ => string.Empty
        };
    }

    private static string MatchWeightStrict(FontWeight fontWeight)
    {
        return fontWeight switch
        {
            FontWeight.Normal => string.Empty,
            _ => fontWeight.ToString()
        };
    }

    private static string MatchWeightMiddle(FontWeight fontWeight)
    {
        return fontWeight switch
        {
            FontWeight.Light => nameof(FontWeight.Light),
            FontWeight.Medium => nameof(FontWeight.Medium),
            < FontWeight.Light => $"{fontWeight + 100}",
            > FontWeight.Medium => $"{fontWeight - 100}",
            _ => string.Empty
        };
    }

    private static string MatchWeightOutward(FontWeight fontWeight)
    {
        return fontWeight switch
        {
            FontWeight.Thin => nameof(FontWeight.Thin),
            FontWeight.Black => nameof(FontWeight.Black),
            < FontWeight.Normal => $"{fontWeight - 100}",
            > FontWeight.Normal => $"{fontWeight + 100}",
            _ => string.Empty
        };
    }

    private static string BuildStylizedFontName(string fontName, string fontWeight, string fontStyle)
    {
        var stylizedFontName = new StringBuilder(fontName);

        if (!string.IsNullOrEmpty(fontWeight))
        {
            stylizedFontName.Append($" {fontWeight}");
        }

        if (!string.IsNullOrEmpty(fontStyle))
        {
            stylizedFontName.Append($" {fontStyle}");
        }

        return stylizedFontName.ToString();
    }
}