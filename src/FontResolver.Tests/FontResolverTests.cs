using System.Runtime.InteropServices;

namespace FontResolution.Tests;

[TestClass]
public sealed class FontResolverTests
{
    [TestCleanup]
    public void Cleanup()
    {
        FontResolver.ClearCache();
    }

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
    [DataRow("")]
    [DataRow("  ")]
    public void Resolve_EmptyFontName_NullIsReturned(string fontName)
    {
        // Arrange
        var style = new FontAttributes();

        // Act
        var strictFont = FontResolver.Resolve(fontName, style);
        var closestFont = FontResolver.Resolve(fontName, style, FontResolveStrategy.Closest);

        // Assert
        Assert.IsNull(strictFont, "No font should be resolved for an empty name.");
        Assert.IsNull(closestFont, "No font should be resolved for an empty name, also with Closest.");
    }

    [TestMethod]
    public void RegisterCustomFontDirectory_DirectoryAdded_FontIsResolvedFromCustomDirectory()
    {
        // Arrange
        var style = new FontAttributes { Weight = FontWeight.Normal, Style = FontStyle.Normal };
        var testDirectory = AppContext.BaseDirectory;

        // Act
        FontResolver.RegisterCustomFontDirectory(testDirectory);
        var font = FontResolver.Resolve("Font Stub", style);

        // Assert
        Assert.IsNotNull(font, "Font should not be null for a font");
        Assert.IsNotNull(font.FilePath, "FontFilePath should not be null for a font");
        Assert.IsTrue(File.Exists(font.FilePath), $"Font file should exist at path: {font}");
    }

    [TestMethod]
    [DoNotParallelize]
    public void RegisterCustomFontDirectory_CalledDuringAScan_DirectoryIsScanned()
    {
        // Registering at different points during a scan, one of them lands after the scan read the
        // directory list and before it stored its result
        for (var delay = 0; delay < 100; delay += 5)
        {
            // Arrange
            var directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(directory);
            File.Copy(
                Path.Combine(AppContext.BaseDirectory, "Font Stub.ttf"),
                Path.Combine(directory, "Font Stub.ttf")
            );

            try
            {
                // Act
                FontResolver.ClearCache();
                var scanning = Task.Run(FontResolver.ResolveAll);
                Thread.Sleep(delay);
                FontResolver.RegisterCustomFontDirectory(directory);
                scanning.Wait();
                var fontDirectories = FontResolver
                    .ResolveAll()
                    .Select(font => Path.GetDirectoryName(font.FilePath))
                    .ToList();

                // Assert
                Assert.Contains(
                    directory,
                    fontDirectories,
                    $"Fonts in {directory} registered after {delay} ms should be found."
                );
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }
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

    [TestMethod]
    [DoNotParallelize]
    public void ClearCache_FontsAreCleared_FontIsResolvedAgain()
    {
        // Arrange
        var fontName = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "DejaVu Sans" : "Arial";
        var style = new FontAttributes { Weight = FontWeight.Normal, Style = FontStyle.Normal };

        // Act
        var fontBeforeClear = FontResolver.Resolve(fontName, style);
        FontResolver.ClearCache();
        var fontAfterClear = FontResolver.Resolve(fontName, style);

        // Assert
        Assert.IsNotNull(fontBeforeClear, "Font should not be null before clearing cache.");
        Assert.IsNotNull(fontAfterClear, "Font should not be null after clearing cache.");
        Assert.AreEqual(
            fontBeforeClear.FilePath,
            fontAfterClear.FilePath,
            "Font file paths should be the same before and after clearing cache."
        );
    }
}
