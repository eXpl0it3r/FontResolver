using System.Runtime.InteropServices;

namespace FontResolver.Tests
{
    [TestClass]
    public sealed class FontResolverTests
    {
        [TestMethod]
        public void Resolve_FontExists_FontPathIsReturned()
        {
            // Arrange
            var fontName = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "DejaVu Sans" : "Arial";

            var style = new FontStyle { Bold = false, Italic = false };

            // Act
            var fontPath = FontResolver.Resolve(fontName, style);

            // Assert
            Assert.IsNotNull(fontPath, "Font path should not be null for an existing font.");
            Assert.IsTrue(File.Exists(fontPath), $"Font file should exist at path: {fontPath}");
        }

        [TestMethod]
        public void Resolve_FontDoesNotExist_NullIsReturned()
        {
            // Arrange
            const string fontName = "RandomFontNameThatDoesNotExist";

            var style = new FontStyle { Bold = false, Italic = false };

            // Act
            var fontPath = FontResolver.Resolve(fontName, style);

            // Assert
            Assert.IsNull(fontPath, "Font path should be null for a non-existing font.");
        }

        [TestMethod]
        public void RegisterCustomFontDirectory_DirectoryAdded_FontIsResolvedFromCustomDirectory()
        {
            // Arrange
            var style = new FontStyle { Bold = false, Italic = false };

            // Act
            FontResolver.RegisterCustomFontDirectory(Directory.GetCurrentDirectory());
            var fontPath = FontResolver.Resolve("Font Stub", style);
            
            // Assert
            Assert.IsNotNull(fontPath, "Font path should not be null for a font");
            Assert.IsTrue(File.Exists(fontPath), $"Font file should exist at path: {fontPath}");
        }

        [TestMethod]
        public void DiscoverFontFamilies_FontsExists_KnownFontsAreReturned()
        {
            // Arrange
            var knownFonts = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? new[] { "DejaVuSans" }
                : new[] { "Arial", "Times New Roman", "Courier New" };

            // Act
            var discoveredFonts = FontResolver.DiscoverFontFamilies();

            // Assert
            foreach (var knownFont in knownFonts)
            {
                Assert.Contains(knownFont, discoveredFonts, $"Discovered fonts should contain known font: {knownFont}");
            }
        }

        [TestMethod]
        public void DiscoverFontFamilies_ResolveFont_FontIsResolved()
        {
            // Arrange & Act
            var discoveredFonts = FontResolver.DiscoverFontFamilies();
            var resolvedFont = FontResolver.Resolve(discoveredFonts.First(), new FontStyle());

            // Assert
            Assert.IsNotEmpty(discoveredFonts, "System should have fonts.");
            Assert.IsNotNull(resolvedFont, $"System font family '{discoveredFonts.First()}' should have resolved.");
        }
    }
}
