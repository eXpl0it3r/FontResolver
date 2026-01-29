using System.Text;

namespace FontResolution;

/// <summary>
/// Represents extracted font metadata from a TTF/OTF file.
/// </summary>
internal class FontMetadata
{
    // Legacy names (nameID 1 & 2)
    public string? FamilyName { get; set; }
    public string? Subfamily { get; set; }
    
    // Full and PostScript names
    public string? FullName { get; set; }
    public string? PostScriptName { get; set; }
    
    // Preferred/Typographic names (nameID 16 & 17)
    public string? PreferredFamily { get; set; }
    public string? PreferredSubfamily { get; set; }
    
    // Derived style attributes
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public string? Weight { get; set; }
    public string? Width { get; set; }
}

/// <summary>
/// Simple parser for TTF and OTF font files to extract the font family name and style.
/// </summary>
internal static class FontParser
{
    /// <summary>
    /// Tries to extract detailed font metadata from a TTF or OTF file.
    /// </summary>
    /// <param name="fontFilePath">Path to the font file</param>
    /// <returns>Font metadata if found, otherwise null</returns>
    public static FontMetadata? ExtractFontFamily(string fontFilePath)
    {
        return ExtractFontMetadata(fontFilePath);
    }

    /// <summary>
    /// Tries to extract detailed font metadata from a TTF or OTF file.
    /// </summary>
    /// <param name="fontFilePath">Path to the font file</param>
    /// <returns>Font metadata if found, otherwise null</returns>
    private static FontMetadata? ExtractFontMetadata(string fontFilePath)
    {
        try
        {
            using var fs = new FileStream(fontFilePath, FileMode.Open, FileAccess.Read);
            var bytes = new byte[fs.Length];
            fs.Read(bytes, 0, bytes.Length);

            // Read the font header (big-endian)
            uint scalarType = ReadUInt32BE(bytes, 0);
            
            // Verify it's a valid TTF/OTF file
            if (scalarType != 0x00010000 && // TrueType version 1.0
                scalarType != 0x74727565 && // "true" - TrueType
                scalarType != 0x4F54544F) // "OTTO" - OpenType with CFF outline
            {
                return null;
            }

            ushort tableCount = ReadUInt16BE(bytes, 4);
            
            // Find the "name" table
            long nameTableOffset = -1;
            int tableRecordOffset = 12;
            
            for (int i = 0; i < tableCount; i++)
            {
                string tag = Encoding.ASCII.GetString(bytes, tableRecordOffset, 4);
                uint offset = ReadUInt32BE(bytes, tableRecordOffset + 8);

                if (tag == "name")
                {
                    nameTableOffset = offset;
                    break;
                }

                tableRecordOffset += 16;
            }

            if (nameTableOffset == -1)
            {
                return null;
            }

            var metadata = new FontMetadata();

            // Parse the name table
            int nameTablePos = (int)nameTableOffset;
            ushort format = ReadUInt16BE(bytes, nameTablePos);
            ushort count = ReadUInt16BE(bytes, nameTablePos + 2);
            ushort stringDataOffset = ReadUInt16BE(bytes, nameTablePos + 4);

            // Track which values have been set with English entries
            var hasEnglishValue = new HashSet<int>();
            
            // First pass: Look for English entries
            int nameRecordOffset = nameTablePos + 6;
            for (int i = 0; i < count; i++)
            {
                ushort platformID = ReadUInt16BE(bytes, nameRecordOffset);
                ushort encodingID = ReadUInt16BE(bytes, nameRecordOffset + 2);
                ushort languageID = ReadUInt16BE(bytes, nameRecordOffset + 4);
                ushort nameID = ReadUInt16BE(bytes, nameRecordOffset + 6);
                ushort length = ReadUInt16BE(bytes, nameRecordOffset + 8);
                ushort offset = ReadUInt16BE(bytes, nameRecordOffset + 10);

                // Filter for English language entries:
                // Platform 3 (Windows): languageID 0x0409 (1033) = US English
                // Platform 1 (Macintosh): languageID 0 = English
                bool isEnglish = (platformID == 3 && languageID == 0x0409) || 
                                 (platformID == 1 && languageID == 0);

                if ((platformID == 3 || platformID == 1) && IsRelevantNameID(nameID))
                {
                    int stringPos = (int)nameTableOffset + stringDataOffset + offset;
                    
                    if (stringPos + length <= bytes.Length)
                    {
                        byte[] nameBytes = new byte[length];
                        Array.Copy(bytes, stringPos, nameBytes, 0, length);
                        
                        var decodedString = DecodeNameTableString(nameBytes, platformID, encodingID);

                        if (!string.IsNullOrEmpty(decodedString) && decodedString != null)
                        {
                            // If English, set the value and mark as having English
                            if (isEnglish)
                            {
                                if (SetMetadataValue(metadata, nameID, decodedString))
                                {
                                    hasEnglishValue.Add(nameID);
                                }
                            }
                            // If not English but we haven't found an English entry yet, use it as fallback
                            else if (!hasEnglishValue.Contains(nameID))
                            {
                                SetMetadataValue(metadata, nameID, decodedString);
                            }
                        }
                    }
                }

                nameRecordOffset += 12;
            }

            // Return metadata if we at least have a family name
            if (!string.IsNullOrEmpty(metadata.FamilyName))
            {
                return metadata;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsRelevantNameID(ushort nameID)
    {
        return nameID == 1 || nameID == 2 || nameID == 4 || nameID == 6 || nameID == 16 || nameID == 17;
    }

    private static bool SetMetadataValue(FontMetadata metadata, ushort nameID, string value)
    {
        switch (nameID)
        {
            case 1: // Legacy Family name
                if (metadata.FamilyName == null)
                {
                    metadata.FamilyName = value;
                    return true;
                }
                return false;
            case 2: // Legacy Subfamily
                if (metadata.Subfamily == null)
                {
                    metadata.Subfamily = value;
                    ParseStyleFromSubfamily(metadata, value);
                    return true;
                }
                return false;
            case 4: // Full font name
                if (metadata.FullName == null)
                {
                    metadata.FullName = value;
                    return true;
                }
                return false;
            case 6: // PostScript name
                if (metadata.PostScriptName == null)
                {
                    metadata.PostScriptName = value;
                    return true;
                }
                return false;
            case 16: // Preferred Family (Typographic Family)
                if (metadata.PreferredFamily == null)
                {
                    metadata.PreferredFamily = value;
                    return true;
                }
                return false;
            case 17: // Preferred Subfamily (Typographic Subfamily)
                if (metadata.PreferredSubfamily == null)
                {
                    metadata.PreferredSubfamily = value;
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    private static void ParseStyleFromSubfamily(FontMetadata metadata, string subfamily)
    {
        var lower = subfamily.ToLowerInvariant();

        // Check for bold variants
        if (lower.Contains("bold"))
        {
            metadata.IsBold = true;
        }

        // Check for italic/oblique variants
        if (lower.Contains("italic") || lower.Contains("oblique"))
        {
            metadata.IsItalic = true;
        }

        // Extract weight information
        if (lower.Contains("black"))
            metadata.Weight = "Black";
        else if (lower.Contains("extrabold") || lower.Contains("ultra bold"))
            metadata.Weight = "ExtraBold";
        else if (lower.Contains("bold"))
            metadata.Weight = "Bold";
        else if (lower.Contains("semibold") || lower.Contains("demibold"))
            metadata.Weight = "SemiBold";
        else if (lower.Contains("medium"))
            metadata.Weight = "Medium";
        else if (lower.Contains("extralight") || lower.Contains("ultra light"))
            metadata.Weight = "ExtraLight";
        else if (lower.Contains("light"))
            metadata.Weight = "Light";
        else if (lower.Contains("thin"))
            metadata.Weight = "Thin";
        else if (lower.Contains("regular") || lower.Contains("normal"))
            metadata.Weight = "Regular";

        // Extract width information
        if (lower.Contains("condensed"))
            metadata.Width = "Condensed";
        else if (lower.Contains("semicondensed"))
            metadata.Width = "SemiCondensed";
        else if (lower.Contains("expanded"))
            metadata.Width = "Expanded";
        else if (lower.Contains("semiexpanded"))
            metadata.Width = "SemiExpanded";
        else if (lower.Contains("ultracondensed") || lower.Contains("extra condensed"))
            metadata.Width = "UltraCondensed";
        else if (lower.Contains("ultraexpanded") || lower.Contains("extra expanded"))
            metadata.Width = "UltraExpanded";
    }


    private static ushort ReadUInt16BE(byte[] data, int offset)
    {
        if (offset + 1 >= data.Length)
            return 0;
        return (ushort)((data[offset] << 8) | data[offset + 1]);
    }

    private static uint ReadUInt32BE(byte[] data, int offset)
    {
        if (offset + 3 >= data.Length)
            return 0;
        return ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) |
               ((uint)data[offset + 2] << 8) | data[offset + 3];
    }

    private static string? DecodeNameTableString(byte[] data, ushort platformID, ushort encodingID)
    {
        try
        {
            return platformID switch
            {
                // Windows platform
                3 => encodingID switch
                {
                    1 => DecodeUTF16BE(data),
                    _ => null
                },
                // Macintosh platform
                1 => encodingID switch
                {
                    0 => Encoding.ASCII.GetString(data).TrimEnd('\0'),
                    _ => null
                },
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private static string DecodeUTF16BE(byte[] data)
    {
        var chars = new List<char>();
        for (int i = 0; i < data.Length - 1; i += 2)
        {
            char c = (char)((data[i] << 8) | data[i + 1]);
            if (c != '\0')
                chars.Add(c);
        }
        return new string(chars.ToArray());
    }
}
