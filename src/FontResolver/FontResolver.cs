using FontResolution.Discovery;

namespace FontResolution;

public static class FontResolver
{
    private readonly static object InitializationLock = new();
    
    private static FontDiscoveryService? _discoveryService;

    public static string? Resolve(string fontName, FontResolveStrategy resolveStrategy = FontResolveStrategy.Strict)
    {
        return Resolve(fontName, new FontAttributes(), resolveStrategy);
    }
    
    public static string? Resolve(string fontName, FontAttributes fontAttributes, FontResolveStrategy resolveStrategy = FontResolveStrategy.Strict)
    {
        EnsureInitialized();

        return resolveStrategy switch
        {
            FontResolveStrategy.Closest => FontNameResolver.StylizeFontNameClosest(fontName, fontAttributes)
                .Select(stylizedFontName => _discoveryService!.ResolveFontFilePath(stylizedFontName))
                .FirstOrDefault(),
            _ => _discoveryService!.ResolveFontFilePath(FontNameResolver.StylizeFontNameStrict(fontName, fontAttributes))
        };
    }

    public static List<string> DiscoverFontFamilies()
    {
        EnsureInitialized();
        return _discoveryService!.DiscoverFontFamilies();
    }

    public static void RegisterCustomFontDirectory(string fontDirectory)
    {
        EnsureInitialized();
        _discoveryService!.RegisterCustomFontDirectory(fontDirectory);
    }

    private static void EnsureInitialized()
    {
        if (_discoveryService is not null)
        {
            return;
        }

        lock (InitializationLock)
        {
            if (_discoveryService is not null)
            {
                return;
            }

            _discoveryService = new FontDiscoveryService();
        }
    }
}