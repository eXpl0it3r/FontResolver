namespace FontResolution.Discovery;

internal class CachedFontEntry
{
    public string FilePath { get; set; } = string.Empty;
    public FontMetadata? Metadata { get; set; }
    public string OriginalName { get; set; } = string.Empty;
}