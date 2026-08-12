namespace FontResolution;

public class FontMetadata
{
    public string FilePath { get; set; } = string.Empty;

    public string Family { get; set; } = string.Empty;
    public string Subfamily { get; set; } = string.Empty;

    public string PreferredFamily { get; set; } = string.Empty;
    public string PreferredSubfamily { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
    public string PostScriptName { get; set; } = string.Empty;

    public FontAttributes Attributes { get; set; } = new();
}
