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
            var fontName = "Arial";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                fontName = "DejaVu Sans";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                fontName = "Helvetica";
            }

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
            var fontPath = FontResolver.Resolve("FontStub", style);
            
            // Assert
            Assert.IsNotNull(fontPath, "Font path should not be null for a font");
            Assert.IsTrue(File.Exists(fontPath), $"Font file should exist at path: {fontPath}");
        }
    }
}
