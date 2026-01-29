using System.Runtime.InteropServices;
using ConsoleAppFramework;
using FontResolution;
using FontResolution.Discovery;
using FontResolution.Parsing;

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
            var font = FontResolver.Resolve(fontFamily, new FontAttributes());

            if (font is null)
            {
                Console.WriteLine($"Could not resolve font '{fontFamily}'");
                continue;
            }

            var fontMetadata = FontParser.ExtractFontMetadata(font);
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
            fontDirectories.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData/Local/Microsoft/Windows/Fonts"));
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
            var fontFiles = FontDiscoveryService.SupportedFontExtensions.SelectMany(e => Directory.GetFiles(directory, $"*{e}", SearchOption.AllDirectories));

            foreach (var fontFile in fontFiles)
            {
                var fontMetadata = FontParser.ExtractFontMetadata(fontFile);

                if (fontMetadata is null)
                {
                    Console.WriteLine($"Could not extract metadata for font file '{fontFile}'");
                    continue;
                }
                
                Console.WriteLine($"Found {fontFile}");
                Console.WriteLine($"\tFamilyName: {fontMetadata.FamilyName}");
                Console.WriteLine($"\tSubfamily: {fontMetadata.Subfamily}");
                Console.WriteLine($"\tPreferredFamily: {fontMetadata.PreferredFamily}");
                Console.WriteLine($"\tPreferredSubfamily: {fontMetadata.PreferredSubfamily}");
                Console.WriteLine($"\tFullName: {fontMetadata.FullName}");
                Console.WriteLine($"\tPostScriptName: {fontMetadata.PostScriptName}");
                Console.WriteLine($"\tWeight: {fontMetadata.Attributes?.Weight}");
                Console.WriteLine($"\tStyle: {fontMetadata.Attributes?.Style}");
                Console.WriteLine($"\tWidth: {fontMetadata.Attributes?.Width}");
                Console.WriteLine();
            }
        }
    }
}