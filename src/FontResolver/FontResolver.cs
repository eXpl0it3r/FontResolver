using FontResolution.FileResolution;
using FontResolution.Parsing;

namespace FontResolution;

public static class FontResolver
{
    private static readonly object InitializationLock = new();
    private static readonly object CacheLock = new();

    private static FontFileResolver? _fontFileResolver;
    private static List<FontMetadata> _fontMetadataCache = [];

    public static FontMetadata? Resolve(
        string fontName,
        FontAttributes fontAttributes,
        FontResolveStrategy resolveStrategy = FontResolveStrategy.Strict,
        StringComparison stringComparison = StringComparison.Ordinal
    )
    {
        EnsureInitialized();

        var cache = GetFontMetadataCache();

        return FontMatcher.Resolve(
            _fontMetadataCache,
            fontName,
            fontAttributes,
            resolveStrategy,
            stringComparison
        );
    }

    public static IReadOnlyList<FontMetadata> ResolveAll()
    {
        EnsureInitialized();

        return GetFontMetadataCache();
    }

    public static void RegisterCustomFontDirectory(string fontDirectory)
    {
        EnsureInitialized();

        _fontFileResolver!.RegisterCustomFontDirectory(fontDirectory);
        ClearCache();
    }

    public static void ClearCache()
    {
        Interlocked.Exchange(ref _fontMetadataCache, []);
    }

    private static void EnsureInitialized()
    {
        if (_fontFileResolver is not null)
        {
            return;
        }

        lock (InitializationLock)
        {
            if (_fontFileResolver is not null)
            {
                return;
            }

            _fontFileResolver = new FontFileResolver();
        }
    }

    private static List<FontMetadata> GetFontMetadataCache()
    {
        var cache = Volatile.Read(ref _fontMetadataCache);

        if (cache.Count != 0)
        {
            return cache;
        }

        lock (CacheLock)
        {
            cache = _fontMetadataCache;

            if (cache.Count != 0)
            {
                return cache;
            }

            var fontInfos = _fontFileResolver!.ResolveFiles();

            var newCache = fontInfos
                .Select(fontInfo =>
                {
                    var metadata = FontParser.ExtractFontMetadata(fontInfo.FilePath);

                    if (metadata is not null)
                    {
                        return metadata;
                    }

                    return new FontMetadata
                    {
                        FilePath = fontInfo.FilePath,
                        Family = fontInfo.Name,
                        Subfamily = string.Empty,
                        PreferredFamily = fontInfo.Name,
                        PreferredSubfamily = string.Empty,
                        FullName = fontInfo.Name,
                        PostScriptName = Path.GetFileNameWithoutExtension(fontInfo.FilePath),
                        Attributes = new FontAttributes(),
                    };
                })
                .GroupBy(metadata => metadata.FilePath, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            Volatile.Write(ref _fontMetadataCache, newCache);

            return newCache;
        }
    }
}
