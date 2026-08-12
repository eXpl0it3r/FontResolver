using ConsoleAppFramework;
using FontResolution;

var app = ConsoleApp.Create();
app.Add<FontResolverCli>();
app.Run(args);

public class FontResolverCli
{
    [Command("resolve")]
    public void Resolve(string fontName, string? fontDirectory = null)
    {
        if (!string.IsNullOrEmpty(fontDirectory))
        {
            FontResolver.RegisterCustomFontDirectory(fontDirectory);
        }

        var font = FontResolver.Resolve(fontName, new FontAttributes());

        if (font?.FilePath is null)
        {
            Console.WriteLine($"Could not resolve font '{fontName}'");
            return;
        }

        Console.WriteLine(font.FilePath);
    }

    [Command("files")]
    public void ListFontFiles()
    {
        var fontFiles = FontResolver.ResolveAll().Select(f => f.FilePath).Distinct().ToList();

        if (fontFiles.Count == 0)
        {
            Console.WriteLine("No font files found");
            return;
        }

        foreach (var fontFile in fontFiles)
        {
            Console.WriteLine(fontFile);
        }
    }

    [Command("families")]
    public void ListFontFamilies()
    {
        var fontFamilies = FontResolver
            .ResolveAll()
            .Select(f => f.Family)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (fontFamilies.Count == 0)
        {
            Console.WriteLine("No font families found");
            return;
        }

        foreach (var fontFamily in fontFamilies)
        {
            Console.WriteLine(fontFamily);
        }
    }

    [Command("all")]
    public void ResolveAll()
    {
        var fonts = FontResolver.ResolveAll();

        if (fonts.Count == 0)
        {
            Console.WriteLine("No fonts found");
            return;
        }

        foreach (var font in fonts)
        {
            Console.WriteLine($"Found {font.FilePath}");

            Console.WriteLine($"\tFamily: {font.Family}");
            Console.WriteLine($"\tSubfamily: {font.Subfamily}");

            Console.WriteLine($"\tPreferredFamily: {font.PreferredFamily}");
            Console.WriteLine($"\tPreferredSubfamily: {font.PreferredSubfamily}");

            Console.WriteLine($"\tFullName: {font.FullName}");
            Console.WriteLine($"\tPostScriptName: {font.PostScriptName}");

            Console.WriteLine($"\tWeight: {font.Attributes.Weight}");
            Console.WriteLine($"\tStyle: {font.Attributes.Style}");
            Console.WriteLine($"\tWidth: {font.Attributes.Width}");
            Console.WriteLine();
        }
    }
}
