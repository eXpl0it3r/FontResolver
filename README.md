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

## Process

- Only `*.ttf` and `*.otf` files are supported
  - TrueType Collections (`*.ttc`) are not supported
- Discovery
  - Windows: Available fonts are retrieved from the registry
  - Linux: If available, fonts are discovered using FontConfig's `fc list` 
  - Platform specific directories are being scanned
  - Registered custom directories are being scanned
- Parsing
  - A basic TTF/OTF parser extracts the desired font metadata
  - As a fallback the font metadata is heuristically extracted from the font name
- Resolving
  - The name matching priority is:
    - Preferred Family Name
    - Legacy Family Name
    - Full Name
    - PostScript Name
    - Match Family
    - Match Style
    - Match Weight
    - Match Width
    - Fallback
  - The resolve strategies are:
    - `Strict`: Name, Style, Weight, and Width need to match
    - `Closest`:
      - Name should match
      - Style: `Oblique` is resolved as `Italic`, when there is no `Italic`
      - Weight: One category above or below is picked, when there is no match
      - Width: One category above or below is picked, when there is no match

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
using FontResolver.PdfSharp;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

// ...

// Before rendering the PDF document call
FontResolverPdfSharp.Register();

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

### Register Custom Font Directories

You can register custom font directories to be searched by the font resolver:

```csharp
FontResolver.RegisterFontDirectory("path/to/custom/directory/with/fonts");

// Or for PDFsharp
FontResolverPdfSharp.RegisterFontDirectory("path/to/custom/directory/with/fonts");
```

### Discover Font Families

Discover all the font families available on the system:

```csharp
var fontFamilies = FontResolver.DiscoverFontFamilies();
```

## License

FontResolver is licensed under the MIT license, see the [LICENSE](LICENSE) file.
