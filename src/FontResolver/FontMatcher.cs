namespace FontResolution;

internal static class FontMatcher
{
    public static FontMetadata? Resolve(
        IEnumerable<FontMetadata> fonts,
        string name,
        FontAttributes attributes,
        FontResolveStrategy strategy,
        StringComparison stringComparison
    )
    {
        var candidates = fonts
            .Select(font => new
            {
                Font = font,
                NameScore = GetNameScore(font, name, stringComparison),
            })
            .Where(x => x.NameScore > 0);

        if (strategy == FontResolveStrategy.Strict)
        {
            return candidates
                .Where(x => AttributesEqual(x.Font.Attributes, attributes))
                .OrderByDescending(x => x.NameScore)
                .Select(x => x.Font)
                .FirstOrDefault();
        }

        return candidates
            .OrderByDescending(x => x.NameScore)
            .ThenByDescending(x => GetAttributeScore(x.Font.Attributes, attributes))
            .Select(x => x.Font)
            .FirstOrDefault();
    }

    private static int GetNameScore(
        FontMetadata font,
        string name,
        StringComparison stringComparison
    )
    {
        var normalizedName = Normalize(name);

        if (Normalize(font.PreferredFamily).Equals(normalizedName, stringComparison))
        {
            return 100;
        }

        if (Normalize(font.Family).Equals(normalizedName, stringComparison))
        {
            return 95;
        }

        // Useful when the supplied name corresponds to the family + subfamily representation.
        if (
            Normalize($"{font.PreferredFamily} {font.PreferredSubfamily}")
                .Equals(normalizedName, stringComparison)
        )
        {
            return 90;
        }

        if (Normalize($"{font.Family} {font.Subfamily}").Equals(normalizedName, stringComparison))
        {
            return 85;
        }

        if (Normalize(font.FullName).Equals(normalizedName, stringComparison))
        {
            return 80;
        }

        if (Normalize(font.PostScriptName).Equals(normalizedName, stringComparison))
        {
            return 75;
        }

        return 0;
    }

    private static bool AttributesEqual(FontAttributes left, FontAttributes right)
    {
        return left.Weight == right.Weight
            && left.Style == right.Style
            && left.Width == right.Width;
    }

    private static int GetAttributeScore(FontAttributes actual, FontAttributes requested)
    {
        var weightDistance = Math.Abs((int)actual.Weight - (int)requested.Weight);

        var widthDistance = Math.Abs((int)actual.Width - (int)requested.Width);

        var styleScore =
            actual.Style == requested.Style
                ? 100
                : actual.Style switch
                {
                    FontStyle.Italic when requested.Style == FontStyle.Oblique => 50,
                    FontStyle.Oblique when requested.Style == FontStyle.Italic => 50,
                    _ => 0,
                };

        // Priority: Style matters most, then weight, then width
        return styleScore * 1000 - weightDistance - widthDistance;
    }

    private static string Normalize(string value)
    {
        return value.Trim();
    }
}
