# Font Resolver

Font Resolver is a cross-platform library to resolve fonts based on their font names.

The library was originally developed to be used with [PDFsharp](https://github.com/empira/PDFsharp).
However, since resolving font files cross-platform can be useful in general, two separate packages have been created:

- [`FontResolver`](https://www.nuget.org/packages/FontResolver) - Generic font resolver
- [`FontResolver.PdfSharp`](https://www.nuget.org/packages/FontResolver.PdfSharp) - Font resolver implementing the PDFsharp interface

## Goals

- Resolve fonts cross-platform
- Keep the dependency graph minimal
- Remain .NET Standard 2.0 compatible
- Turn contributors into maintainers

## Install

Add the NuGet package to your project:

```powershell
dotnet add package FontResolver
```

If you intend to use FontResolver with PDFsharp, use the `FontResolver.PdfSharp` package (which depends on `FontResolver`) instead:

```powershell
dotnet add package FontResolver.PdfSharp
```

## Usage

### Standalone

```csharp
using FontResolution;

// ...

var attributes = new FontAttributes();
var font = FontResolver.Resolve("Arial", attributes with { Weight = FontWeight.Bold });
```

### PDFsharp

```csharp
using FontResolution.PdfSharp;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

// ...

// Before rendering the PDF document call
var fontResolver = FontResolverPdfSharp.Register();

// Or alternatively register the font resolver yourself
GlobalFontSettings.FontResolver = new FontResolverPdfSharp();

// ...

var migraDocDocument = new Document();
migraDocDocument.AddSection();
// ...

var renderer = new PdfDocumentRenderer
{
    Document = migraDocDocument
};
renderer.RenderDocument();

var pdfDocument = renderer.PdfDocument;
pdfDocument.Save("file.pdf");
```

### Resolve Strategy

Sometimes you want a very specific font and sometimes you're okay with a font that closests matches the requested font:

```csharp
using FontResolution;

var attributes = new FontAttributes();

// Returns `null` if Arial Narrow isn't installed
var strictFontMatch = FontResolver.Resolve("Arial Narrow", attributes, FontResolveStrategy.Strict);

// May return Arial or similar if Arial Narrow isn't installed
var closestFontMatch = FontResolver.Resolve("Arial Narrow", attributes, FontResolveStrategy.Closest);

// Influence the string comparison if you can't trust the input
var broadestFontMatch = FontResolver.Resolve("arial narrow", attributes, FontResolveStrategy.Closest, StringComparison.InvariantCultureIgnoreCase);
```

Or for PDFsharp:

```csharp
using FontResolution;

var fontResolverPdfSharp = FontResolverPdfSharp.Register();

// Uses the FallbackFont if the specific font isn't installed
fontResolverPdfSharp.ResolveStrategy = FontResolveStrategy.Strict;

// May return a similar font famil if the specific font isn't installed
fontResolverPdfSharp.ResolveStrategy = FontResolveStrategy.Closest;

// Influence the string comparison if you can't trust the input
fontResolverPdfSharp.ResolveStrategy = FontResolveStrategy.Closest;
fontResolverPdfSharp.StringComparison = StringComparison.InvariantCultureIgnoreCase;
```

### Register Custom Font Directories

You can register custom font directories to be searched by the font resolver:

```csharp
using FontResolution;

FontResolver.RegisterCustomFontDirectory("path/to/custom/directory/with/fonts");
```

Or for PDFsharp:

```csharp
using FontResolution.PdfSharp;

var fontResolverPdfSharp = FontResolverPdfSharp.Register();

fontResolverPdfSharp.RegisterCustomFontDirectory("path/to/custom/directory/with/fonts");
```

### Resolve All Existing Fonts

Resolve all the fonts available on the system:

```csharp
using FontResolution;

var fonts = FontResolver.ResolveAll();

foreach (var font in fonts)
{
    Console.WriteLine($"Family: {font.Family}");
}
```

### Clear the Cache

After installing new system fonts yourself, it may be necessary to clear FontResolver's internal cache.

```csharp
using FontResolution;

FontResolver.ClearCache();
```

Or for PDFsharp:

```csharp
using FontResolution.PdfSharp;

var fontResolverPdfSharp = FontResolverPdfSharp.Register();

fontResolverPdfSharp.ClearCache();
```

> [!NOTE]
> - When clearing the cache for FontResolverPdfSharp, the previously discovered font faces will no longer be returned by `GetFont()`.
> - When adding a custom font directory, the cache is automatically cleared.

## Resolving Process

- Font File Discovery
  - Only `*.ttf` and `*.otf` files are supported
  - TrueType Collections (`*.ttc`) are *not* supported
  - Windows: Available fonts are retrieved from the registry
  - Linux: If available, fonts are discovered using FontConfig's `fc list`
  - Platform specific directories are being scanned
  - Registered custom directories are being scanned
- Font Parsing
  - A basic TTF/OTF parser extracts the desired font metadata
  - As a fallback the font name is used with default attributes
- Font Matching
  - Use a scoring system to determine the best match for the given name and attributes
  - The resolve strategies are:
    - `Strict`: Name, Style, Weight, and Width need to match
    - `Closest`: Font with the highest score is returned

## License

FontResolver is licensed under the MIT license, see the [LICENSE](LICENSE) file.
