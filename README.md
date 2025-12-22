# Font Resolver

Font Resolver is a cross-platform library to resolve font paths based on their font names.

The library was originally developed to be used with [PDFsharp](https://github.com/empira/PDFsharp).
However, since resolving font files cross-platform can be useful in general, two separate packages have been created:

- [`FontResolver`](https://www.nuget.org/packages/FontResolver)
- [`FontResolver.PdfSharp`](https://www.nuget.org/packages/FontResolver.PdfSharp)

## Goals

- Resolve font files cross-platform
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
using FontResolver;

// ...

var style = new FontStyle(bold: false, italic: false);
var font = FontResolver.Resolve("Arial", style with { Bold = true });
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

## License

FontResolver is licensed under the MIT license, see the [LICENSE](LICENSE) file.
