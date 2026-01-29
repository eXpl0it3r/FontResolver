using System.Runtime.InteropServices;

namespace FontResolution.Discovery;

internal class FontDiscoveryService
{
    private readonly FontCache _cache = new();
    private readonly List<string> _customFontDirectories = [];
    private readonly object _discoveryLock = new();
    
    private bool _discoveryCompleted;

    public static string[] SupportedFontExtensions { get; } = [ ".ttf", ".otf" ];

    public string? ResolveFontFilePath(string fontName)
    {
        // Check cache first
        var font = _cache.GetFont(fontName);
        if (font is not null && File.Exists(font.FilePath))
        {
            return font.FilePath;
        }

        // Not in cache or file changed, discover fonts
        EnsureDiscoveryCompleted();

        // Try cache again after discovery
        font = _cache.GetFont(fontName);
        return font?.FilePath;
    }

    public List<string> DiscoverFontFamilies()
    {
        EnsureDiscoveryCompleted();
        return _cache.GetAllFontFamilies();
    }

    public void RegisterCustomFontDirectory(string fontDirectory)
    {
        if (_customFontDirectories.Contains(fontDirectory))
        {
            return;
        }
        
        _customFontDirectories.Add(fontDirectory);
            
        // New font directory resets discovery state
        _discoveryCompleted = false;
    }

    private void EnsureDiscoveryCompleted()
    {
        if (_discoveryCompleted)
        {
            return;
        }

        lock (_discoveryLock)
        {
            if (_discoveryCompleted)
            {
                return;
            }

            PerformFontDiscovery();
            _discoveryCompleted = true;
        }
    }

    private void PerformFontDiscovery()
    {
        // Clear any previous incomplete discovery
        _cache.Clear();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            WindowsFontDiscovery.DiscoverFonts(_cache);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            LinuxFontDiscovery.DiscoverFonts(_cache);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            MacOsFontDiscovery.DiscoverFonts(_cache);
        }

        // Always use directory scan as fallback
        DirectoryFontDiscovery.DiscoverFontsFromDirectories(_cache, _customFontDirectories);
    }
}

