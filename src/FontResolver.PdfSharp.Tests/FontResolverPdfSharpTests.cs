using System.Runtime.InteropServices;
using System.Text;
using MigraDoc;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Fonts;

namespace FontResolution.PdfSharp.Tests;

[TestClass]
public sealed class FontResolverPdfSharpTests
{
    [TestCleanup]
    public void Cleanup()
    {
        FontResolver.ClearCache();
    }

    [TestMethod]
    public void Register_GlobalRegisterUnset_GlobalRegisterIsSet()
    {
        // Arrange & Act
        var fontResolverPdfSharp = FontResolverPdfSharp.Register();

        // Assert
        Assert.IsNotNull(fontResolverPdfSharp, "Font resolver is returned after registration.");
        Assert.IsInstanceOfType<FontResolverPdfSharp>(
            GlobalFontSettings.FontResolver,
            "Global font resolver should have FontResolverPdfSharp type registered."
        );
        Assert.AreEqual(
            GlobalFontSettings.FontResolver,
            fontResolverPdfSharp,
            "Global font resolver is set after registration."
        );
    }

    [TestMethod]
    public void Register_FontExistsAndPdfGenerated_FontIsResolved()
    {
        PredefinedFontsAndChars.ErrorFontName = FontResolverPdfSharp.FallbackFont;

        var migraDocDocument = new Document();
        var section = migraDocDocument.AddSection();
        var paragraph = section.AddParagraph("This is a test");

        paragraph.Format.Font.Name = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            ? "DejaVu Sans"
            : "Arial";

        var renderer = new PdfDocumentRenderer { Document = migraDocDocument };

        // Act
        FontResolverPdfSharp.Register();
        renderer.RenderDocument();
        var pdfDocument = renderer.PdfDocument;

        // Assert
        Assert.IsNotNull(pdfDocument, "PDF document should not be null after rendering.");

        using var stream = new MemoryStream();
        pdfDocument.Save(stream);

        var pdfBytes = stream.ToArray();
        var pdfText = Encoding.ASCII.GetString(pdfBytes);

        Assert.Contains(
            paragraph.Format.Font.Name.Replace(" ", "#20"),
            pdfText,
            "PDF document should contain requested font after rendering."
        );
    }

    [TestMethod]
    public void Register_FontDoesNotExistAndPdfGenerated_FallbackFontIsResolved()
    {
        PredefinedFontsAndChars.ErrorFontName = FontResolverPdfSharp.FallbackFont;

        var migraDocDocument = new Document();
        var section = migraDocDocument.AddSection();
        var paragraph = section.AddParagraph("This is a test");

        paragraph.Format.Font.Name = "RandomFontNameThatDoesNotExist";

        var renderer = new PdfDocumentRenderer { Document = migraDocDocument };

        // Act
        FontResolverPdfSharp.Register();
        renderer.RenderDocument();
        var pdfDocument = renderer.PdfDocument;

        // Assert
        Assert.IsNotNull(pdfDocument, "PDF document should not be null after rendering.");

        using var stream = new MemoryStream();
        pdfDocument.Save(stream);

        var pdfBytes = stream.ToArray();
        var pdfText = Encoding.ASCII.GetString(pdfBytes);

        Assert.Contains(
            FontResolverPdfSharp.FallbackFont,
            pdfText,
            "PDF document should contain fallback font after rendering."
        );
    }

    [TestMethod]
    public void ResolveTypeface_FontExists_FontInfoIsReturned()
    {
        // Arrange
        var fontResolver = new FontResolverPdfSharp();
        var fontName = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "DejaVu Sans" : "Arial";

        // Act
        var fontInfo = fontResolver.ResolveTypeface(fontName, false, false);

        // Assert
        Assert.IsNotNull(fontInfo, "Font info should not be null for an existing font.");
        Assert.StartsWith(
            fontName,
            fontInfo.FaceName,
            "Font face name start matches requested font."
        );
        Assert.EndsWith(
            "Regular",
            fontInfo.FaceName,
            "Font face name end matches requested style."
        );
    }

    [TestMethod]
    public void ResolveTypeface_FontDoesNotExist_FontFallbackIsReturned()
    {
        // Arrange
        var fontResolver = new FontResolverPdfSharp();

        const string fontName = "RandomFontNameThatDoesNotExist";

        // Act
        var fontInfo = fontResolver.ResolveTypeface(fontName, false, false);

        // Assert
        Assert.IsNotNull(fontInfo, "Font info should not be null for the fallback font.");
        Assert.AreEqual(
            $"{FontResolverPdfSharp.FallbackFont} Regular",
            fontInfo.FaceName,
            "Font face name match the fallback font."
        );
    }

    [TestMethod]
    public void GetFont_FontExists_FontDataIsReturned()
    {
        // Arrange
        var fontResolver = new FontResolverPdfSharp();
        var fontName = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "DejaVu Sans" : "Arial";
        var fontInfo = fontResolver.ResolveTypeface(fontName, false, false);

        // Act
        var fontData = fontResolver.GetFont(fontInfo.FaceName);

        // Assert
        Assert.StartsWith(
            fontName,
            fontInfo.FaceName,
            "Resolved font should not be the fallback font."
        );
        Assert.IsNotNull(fontData, "Font data should not be null for the given font.");
        Assert.IsNotEmpty(fontData, "Font data should have content for the given font.");
    }

    [TestMethod]
    public void GetFont_FontDoesNotExist_NullIsReturned()
    {
        // Arrange
        var fontResolver = new FontResolverPdfSharp();

        const string fontName = "RandomFontNameThatDoesNotExist";

        // Act
        var fontData = fontResolver.GetFont(fontName);

        // Assert
        Assert.IsNull(
            fontData,
            "Font data should be null for an explicit request of an unknown font."
        );
    }

    [TestMethod]
    [DataRow("Tuffy Regular")]
    [DataRow("Tuffy Bold")]
    [DataRow("Tuffy Italic")]
    [DataRow("Tuffy Bold Italic")]
    public void GetFont_FallbackFontExists_FontDataIsReturned(string fallbackFont)
    {
        // Arrange
        var fontResolver = new FontResolverPdfSharp();

        // Act
        var fontData = fontResolver.GetFont(fallbackFont);

        // Assert
        Assert.IsNotNull(fontData, "Font data should not be null for the embedded fallback font.");
        Assert.IsNotEmpty(
            fontData,
            "Font data should have content for the embedded fallback font."
        );
    }

    [TestMethod]
    public void RegisterCustomFontDirectory_DirectoryAdded_FontIsResolvedFromCustomDirectory()
    {
        // Arrange
        var fontResolver = new FontResolverPdfSharp();
        var testDirectory = AppContext.BaseDirectory;

        // Act
        fontResolver.RegisterCustomFontDirectory(testDirectory);
        var fontPath = fontResolver.ResolveTypeface("Font Stub", false, false);

        // Assert
        Assert.IsNotNull(fontPath, "Font path should not be null for a font");
        Assert.AreEqual("Font Stub Regular", fontPath.FaceName, "Font file is found");
    }

    [TestMethod]
    public void ClearCache_CacheCleared_FontIsResolvedAgain()
    {
        // Arrange
        var fontResolver = new FontResolverPdfSharp();
        var fontName = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "DejaVu Sans" : "Arial";

        // Act
        var fontInfoBeforeClear = fontResolver.ResolveTypeface(fontName, false, false);
        fontResolver.ClearCache();
        var fontInfoAfterClear = fontResolver.ResolveTypeface(fontName, false, false);

        // Assert
        Assert.IsNotNull(
            fontInfoBeforeClear,
            "Font info should not be null before clearing cache."
        );
        Assert.IsNotNull(fontInfoAfterClear, "Font info should not be null after clearing cache.");
        Assert.AreEqual(
            fontInfoBeforeClear.FaceName,
            fontInfoAfterClear.FaceName,
            "Font face name should remain the same after clearing cache."
        );
    }

    [TestMethod]
    public void ClearCache_GetFont_NullIsReturned()
    {
        // Arrange
        var fontResolver = new FontResolverPdfSharp();
        var fontName = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "DejaVu Sans" : "Arial";
        var fontInfoBeforeClear = fontResolver.ResolveTypeface(fontName, false, false);
        var fontDataBeforeClear = fontResolver.GetFont(fontInfoBeforeClear.FaceName);

        // Act
        fontResolver.ClearCache();
        var fontDataAfterClear = fontResolver.GetFont(fontInfoBeforeClear.FaceName);

        // Assert
        Assert.IsNotNull(
            fontDataBeforeClear,
            "Font data should not be null before clearing cache."
        );
        Assert.IsNull(fontDataAfterClear, "Font data should be null after clearing cache.");
    }
}
