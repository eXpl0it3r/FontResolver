using ConsoleAppFramework;
using FontResolver;

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
            FontResolver.FontResolver.RegisterCustomFontDirectory(fontDirectory);
        }

        var font = FontResolver.FontResolver.Resolve(fontName, new FontStyle());

        if (font is null)
        {
            Console.WriteLine($"Could not resolve font '{fontName}'");
            return;
        }
        
        Console.WriteLine(font);
    }
    
    [Command("list")]
    public void ListFontFamilies()
    {
        var fontFamilies = FontResolver.FontResolver.DiscoverFontFamilies();

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
}