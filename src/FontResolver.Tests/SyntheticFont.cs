using System.Text;

namespace FontResolution.Tests;

public enum NameId : ushort
{
    Family = 1,
    Subfamily = 2,
    TypographicFamily = 16,
    TypographicSubfamily = 17,
}

public enum NameEncoding
{
    UnicodeBmp,
    MacRoman,
    WindowsSymbol,
    WindowsUnicodeBmp,
}

public enum NameLanguage
{
    English,
    German,
}

/// <summary>Builds a minimal font file with only the name and OS/2 tables the font parser reads.</summary>
internal sealed class SyntheticFont
{
    private readonly List<NameRecord> _names = [];
    private FontWeight? _weight;

    public SyntheticFont WithName(
        NameId id,
        string value,
        NameEncoding encoding = NameEncoding.WindowsUnicodeBmp,
        NameLanguage language = NameLanguage.English
    )
    {
        var (platformId, encodingId) = encoding switch
        {
            NameEncoding.UnicodeBmp => ((ushort)0, (ushort)3),
            NameEncoding.MacRoman => ((ushort)1, (ushort)0),
            NameEncoding.WindowsSymbol => ((ushort)3, (ushort)0),
            _ => ((ushort)3, (ushort)1),
        };

        var languageId = (platformId, language) switch
        {
            (0, NameLanguage.English) => (ushort)0,
            (0, _) => throw new ArgumentException("The Unicode platform has no languages.", nameof(language)),
            (1, NameLanguage.German) => (ushort)2,
            (1, _) => (ushort)0,
            (_, NameLanguage.German) => (ushort)0x0407,
            _ => (ushort)0x0409,
        };

        var bytes = encoding == NameEncoding.MacRoman
            ? CodePagesEncodingProvider.Instance.GetEncoding(10000)!.GetBytes(value)
            : Encoding.BigEndianUnicode.GetBytes(value);

        _names.Add(new NameRecord(platformId, encodingId, languageId, (ushort)id, bytes));
        return this;
    }

    public SyntheticFont WithOs2(FontWeight weight)
    {
        _weight = weight;
        return this;
    }

    public string Write(string path)
    {
        var tables = new List<(string Tag, byte[] Data)> { ("name", NameTable()) };
        if (_weight.HasValue)
        {
            tables.Add(("OS/2", Os2Table(_weight.Value)));
        }

        using var stream = new MemoryStream();
        WriteUInt32(stream, 0x00010000);
        WriteUInt16(stream, (ushort)tables.Count);
        WriteUInt16(stream, 0);
        WriteUInt16(stream, 0);
        WriteUInt16(stream, 0);

        var offset = 12 + 16 * tables.Count;
        foreach (var (tag, data) in tables)
        {
            stream.Write(Encoding.ASCII.GetBytes(tag), 0, 4);
            WriteUInt32(stream, 0);
            WriteUInt32(stream, (uint)offset);
            WriteUInt32(stream, (uint)data.Length);
            offset += data.Length;
        }

        foreach (var (_, data) in tables)
        {
            stream.Write(data, 0, data.Length);
        }

        File.WriteAllBytes(path, stream.ToArray());
        return path;
    }

    private byte[] NameTable()
    {
        // The name records have to be sorted like in a real font
        var names = _names
            .OrderBy(n => n.PlatformId)
            .ThenBy(n => n.EncodingId)
            .ThenBy(n => n.LanguageId)
            .ThenBy(n => n.NameId)
            .ToList();

        using var stream = new MemoryStream();
        WriteUInt16(stream, 0);
        WriteUInt16(stream, (ushort)names.Count);
        WriteUInt16(stream, (ushort)(6 + 12 * names.Count));

        var offset = 0;
        foreach (var name in names)
        {
            WriteUInt16(stream, name.PlatformId);
            WriteUInt16(stream, name.EncodingId);
            WriteUInt16(stream, name.LanguageId);
            WriteUInt16(stream, name.NameId);
            WriteUInt16(stream, (ushort)name.Value.Length);
            WriteUInt16(stream, (ushort)offset);
            offset += name.Value.Length;
        }

        foreach (var name in names)
        {
            stream.Write(name.Value, 0, name.Value.Length);
        }

        return stream.ToArray();
    }

    private static byte[] Os2Table(FontWeight weight)
    {
        var table = new byte[78];
        table[4] = (byte)((int)weight >> 8);
        table[5] = (byte)weight;
        // usWidthClass 5 is the normal width
        table[7] = 5;
        return table;
    }

    private static void WriteUInt16(Stream stream, ushort value)
    {
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)value);
    }

    private static void WriteUInt32(Stream stream, uint value)
    {
        WriteUInt16(stream, (ushort)(value >> 16));
        WriteUInt16(stream, (ushort)value);
    }

    private sealed record NameRecord(
        ushort PlatformId,
        ushort EncodingId,
        ushort LanguageId,
        ushort NameId,
        byte[] Value
    );
}
