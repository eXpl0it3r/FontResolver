using MigraDoc;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using System.Runtime.InteropServices;
using System.Text;

namespace FontResolver.PdfSharp.Tests
{
    [TestClass]
    public sealed class FontResolverPdfSharpTests
    {
        [TestMethod]
        public void Register_GlobalRegisterUnset_GlobalRegisterIsSet()
        {
            // Arrange & Act
            FontResolverPdfSharp.Register();

            // Assert
            Assert.IsNotNull(GlobalFontSettings.FontResolver, "Global font resolver should be set after registration.");
            Assert.IsInstanceOfType<FontResolverPdfSharp>(GlobalFontSettings.FontResolver, "Global font resolver should have FontResolverPdfSharp type registered.");
        }

        [TestMethod]
        public void Register_FontExistsAndPdfGenerated_FontIsResolved()
        {
            PredefinedFontsAndChars.ErrorFontName = FontResolverPdfSharp.FallbackFont;

            var migraDocDocument = new Document();
            var section = migraDocDocument.AddSection();
            var paragraph = section.AddParagraph("This is a test");

            paragraph.Format.Font.Name = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "DejaVu Sans" : "Arial";

            var renderer = new PdfDocumentRenderer
            {
                Document = migraDocDocument
            };

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

            Assert.Contains(paragraph.Format.Font.Name.Replace(" ", "#20"), pdfText, "PDF document should contain requested font after rendering.");
        }

        [TestMethod]
        public void Register_FontDoesNotExistAndPdfGenerated_FallbackFontIsResolved()
        {
            PredefinedFontsAndChars.ErrorFontName = FontResolverPdfSharp.FallbackFont;

            var migraDocDocument = new Document();
            var section = migraDocDocument.AddSection();
            var paragraph = section.AddParagraph("This is a test");

            paragraph.Format.Font.Name = "RandomFontNameThatDoesNotExist";

            var renderer = new PdfDocumentRenderer
            {
                Document = migraDocDocument
            };

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

            Assert.Contains(FontResolverPdfSharp.FallbackFont, pdfText, "PDF document should contain fallback font after rendering.");
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
            Assert.AreEqual(fontName, fontInfo.FaceName, "Font face name matches requested font.");
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
            Assert.AreEqual(FontResolverPdfSharp.FallbackFont, fontInfo.FaceName, "Font face name match the fallback font.");
        }

        [TestMethod]
        public void GetFont_FontExists_FontDataIsReturned()
        {
            // Arrange
            var fontResolver = new FontResolverPdfSharp();
            var fontName = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "DejaVu Sans" : "Arial";
            var fontInfo = fontResolver.ResolveTypeface(fontName, false, false);

            // Act
            var fontData = fontResolver.GetFont(fontInfo!.FaceName);

            // Assert
            Assert.StartsWith(fontName, fontInfo.FaceName, "Resolved font should not be the fallback font.");
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
            Assert.IsNull(fontData, "Font data should be null for an explicit request of an unknown font.");
        }

        [TestMethod]
        [DataRow("Tuffy")]
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
            Assert.IsNotEmpty(fontData, "Font data should have content for the embedded fallback font.");
        }

        [TestMethod]
        public void RegisterCustomFontDirectory_DirectoryAdded_FontIsResolvedFromCustomDirectory()
        {
            // Arrange
            var fontResolver = new FontResolverPdfSharp();

            // Act
            FontResolverPdfSharp.RegisterCustomFontDirectory(Directory.GetCurrentDirectory());
            var fontPath = fontResolver.ResolveTypeface("Font Stub", false, false);

            // Assert
            Assert.IsNotNull(fontPath, "Font path should not be null for a font");
            Assert.AreEqual("Font Stub", fontPath.FaceName, "Font file is found");
        }
    }
}
