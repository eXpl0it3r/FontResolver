namespace FontResolution;

public class FontMetadata
{
    public string? FontFilePath { get; set; }
    
    public string? FamilyName { get; set; }
    public string? Subfamily { get; set; }
    
    public string? FullName { get; set; }
    public string? PostScriptName { get; set; }
    
    public string? PreferredFamily { get; set; }
    public string? PreferredSubfamily { get; set; }
    
    public FontAttributes? Attributes { get; set; }
}