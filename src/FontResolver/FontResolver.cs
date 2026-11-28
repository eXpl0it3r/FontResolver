using System.Runtime.InteropServices;
using FontResolution.FileResolution;
using FontResolution.Parsing;

namespace FontResolution;

public static class FontResolver
{
    private static readonly object InitializationLock = new();
    private static readonly object CacheLock = new();

    // Linux file systems are case-sensitive, Windows and macOS ones usually aren't
    private static readonly StringComparer PathComparer = RuntimeInformation.IsOSPlatform(
        OSPlatform.Linux
    )
        ? StringComparer.Ordinal
        : StringComparer.OrdinalIgnoreCase;

    private static FontFileResolver? _fontFileResolver;
    // Null until scanned, an empty list is a valid result on a system without fonts
    private static List<FontMetadata>? _fontMetadataCache;

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
            cache,
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
        // Waits for a scan in progress, which would otherwise store its outdated result after the reset.
        lock (CacheLock)
        {
            Volatile.Write(ref _fontMetadataCache, null);
        }
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

        if (cache is not null)
        {
            return cache;
        }

        lock (CacheLock)
        {
            cache = _fontMetadataCache;

            if (cache is not null)
            {
                return cache;
            }

            var fontInfos = _fontFileResolver!.ResolveFiles();

            // The platform lists and the directory scan return the same files, parse each only once
            var newCache = fontInfos
                .GroupBy(fontInfo => fontInfo.FilePath, PathComparer)
                .Select(group => group.First())
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
                .ToList();

            Volatile.Write(ref _fontMetadataCache, newCache);

            return newCache;
        }
    }
}
