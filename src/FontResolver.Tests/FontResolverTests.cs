using System.Runtime.InteropServices;

namespace FontResolution.Tests;

[TestClass]
public sealed class FontResolverTests
{
    [TestMethod]
    public void Resolve_FontExists_FontPathIsReturned()
    {
        // Arrange
        var fontName = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "DejaVu Sans" : "Arial";
        var style = new FontAttributes { Weight = FontWeight.Normal, Style = FontStyle.Normal };

        // Act
        var font = FontResolver.Resolve(fontName, style);

        // Assert
        Assert.IsNotNull(font, "Font should not be null for an existing font.");
        Assert.IsNotNull(font.FilePath, "Font file path should not be null for an existing font.");
        Assert.IsTrue(File.Exists(font.FilePath), $"Font file should exist at path: {font}");
    }

    [TestMethod]
    public void Resolve_FontDoesNotExist_NullIsReturned()
    {
        // Arrange
        const string fontName = "RandomFontNameThatDoesNotExist";
        var style = new FontAttributes { Weight = FontWeight.Normal, Style = FontStyle.Normal };

        // Act
        var font = FontResolver.Resolve(fontName, style);

        // Assert
        Assert.IsNull(font, "Font should be null for a non-existing font.");
    }

    [TestMethod]
    public void RegisterCustomFontDirectory_DirectoryAdded_FontIsResolvedFromCustomDirectory()
    {
        // Arrange
        var style = new FontAttributes { Weight = FontWeight.Normal, Style = FontStyle.Normal };

        // Act
        FontResolver.RegisterCustomFontDirectory(Directory.GetCurrentDirectory());
        var font = FontResolver.Resolve("Font Stub", style);

        // Assert
        Assert.IsNotNull(font, "Font should not be null for a font");
        Assert.IsNotNull(font.FilePath, "FontFilePath should not be null for a font");
        Assert.IsTrue(File.Exists(font.FilePath), $"Font file should exist at path: {font}");
    }

    [TestMethod]
    public void ResolveAll_FontsExists_KnownFontsAreReturned()
    {
        // Arrange
        var knownFonts =
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? ["DejaVu Sans"]
            : RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new[] { "Arial", "Arial Black", "Times New Roman", "Courier New" }
            : new[] { "Arial", "Times New Roman", "Courier New" };

        // Act
        var discoveredFonts = FontResolver.ResolveAll();

        // Assert
        foreach (var knownFont in knownFonts)
        {
            Assert.Contains(
                knownFont,
                discoveredFonts.Select(f => f.Family),
                $"Discovered fonts should contain known font: {knownFont}"
            );
        }
    }

    [TestMethod]
    public void ResolveAll_ResolveFont_FontIsResolved()
    {
        // Arrange & Act
        var discoveredFonts = FontResolver.ResolveAll();

        foreach (var discoveredFont in discoveredFonts)
        {
            var resolvedFont = FontResolver.Resolve(
                discoveredFont.Family,
                discoveredFont.Attributes
            );

            // Assert
            Assert.IsNotEmpty(discoveredFonts, "System should have fonts.");
            Assert.IsNotNull(
                resolvedFont,
                $"System font family '{discoveredFonts[0]}' should have resolved."
            );
        }
    }
}
