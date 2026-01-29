using System.Runtime.InteropServices;
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

        var font = FontResolver.Resolve(fontName, new FontStyle());

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
        var fontFamilies = FontResolver.DiscoverFontFamilies();

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
        var fontFamilies = FontResolver.DiscoverFontFamilies();

        if (fontFamilies.Count == 0)
        {
            Console.WriteLine("No font families found");
            return;
        }

        foreach (var fontFamily in fontFamilies)
        {
            var font = FontResolver.Resolve(fontFamily, new FontStyle());

            if (font is null)
            {
                Console.WriteLine($"Could not resolve font '{fontFamily}'");
                continue;
            }

            var fontMetadata = FontParser.ExtractFontFamily(font);
            var fontFamilyName = fontMetadata?.FamilyName ?? "Unknown";
            
            Console.WriteLine($"Found {fontFamily} ({fontFamilyName}) at {font}");
        }
    }

    [Command("ls")]
    public void ListDirectories()
    {
        var fontDirectories = new List<string>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            fontDirectories.Add(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            fontDirectories.Add("/System/Library/Fonts");
            fontDirectories.Add("/Library/Fonts");
            fontDirectories.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Fonts"));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            fontDirectories.Add("/usr/share/fonts");
            fontDirectories.Add("/usr/local/share/fonts");
            fontDirectories.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts"));
            fontDirectories.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/fonts"));
        }

        foreach (var directory in fontDirectories.Where(Directory.Exists))
        {
            var fontFiles = Directory.GetFiles(directory, "*.ttf", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(directory, "*.otf", SearchOption.AllDirectories));

            foreach (var fontFile in fontFiles)
            {
                var fontMetadata = FontParser.ExtractFontFamily(fontFile);

                if (fontMetadata is null)
                {
                    Console.WriteLine($"Could not extract metadata for font file '{fontFile}'");
                    continue;
                }
                
                Console.WriteLine($"Found {fontMetadata.FamilyName} / {fontMetadata.Subfamily} / {fontMetadata.PreferredFamily} / {fontMetadata.PreferredSubfamily} / {fontMetadata.FullName} / {fontMetadata.PostScriptName} at {fontFile}");
            }
        }
    }
}