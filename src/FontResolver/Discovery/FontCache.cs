namespace FontResolution.Discovery;

internal class FontCache
{
    private const int CacheExpirationDays = 1;
    
    private readonly object _lockObject = new();
    private readonly Dictionary<string, CachedFontEntry> _cache = new();
    
    private DateTime _cacheLoadTime = DateTime.UtcNow;

    public CachedFontEntry? GetFont(string fontName)
    {
        lock (_lockObject)
        {
            EnsureCacheIsValid();
            
            var normalizedName = NormalizeCacheKey(fontName);
            return _cache.TryGetValue(normalizedName, out var entry) ? entry : null;
        }
    }

    public void SetFont(string fontName, string filePath, FontMetadata? metadata)
    {
        lock (_lockObject)
        {
            EnsureCacheIsValid();
            
            var normalizedName = NormalizeCacheKey(fontName);
            
            _cache[normalizedName] = new CachedFontEntry
            {
                FilePath = filePath,
                Metadata = metadata,
                OriginalName = fontName
            };
        }
    }

    public List<string> GetAllFontFamilies()
    {
        lock (_lockObject)
        {
            EnsureCacheIsValid();
            return _cache.Values.Select(entry => entry.OriginalName).Distinct().ToList();
        }
    }

    public void Clear()
    {
        lock (_lockObject)
        {
            _cache.Clear();
            _cacheLoadTime = DateTime.UtcNow;
        }
    }

    private void EnsureCacheIsValid()
    {
        if (DateTime.UtcNow - _cacheLoadTime > TimeSpan.FromDays(CacheExpirationDays))
        {
            _cache.Clear();
            _cacheLoadTime = DateTime.UtcNow;
        }
    }

    private static string NormalizeCacheKey(string fontName)
    {
        return fontName.ToLowerInvariant().Trim();
    }
}
