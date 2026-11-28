namespace FontResolution.Tests;

// The synthetic fonts are deleted after each test, other tests mustn't see them in between
[TestClass]
[DoNotParallelize]
public sealed class FontParsingTests
{
    private string _directory = string.Empty;

    [TestInitialize]
    public void Initialize()
    {
        _directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_directory);
    }

    [TestCleanup]
    public void Cleanup()
    {
        FontResolver.ClearCache();
        Directory.Delete(_directory, true);
    }

    [TestMethod]
    public void Resolve_LocalizedNamesBeforeEnglish_EnglishFamilyIsResolved()
    {
        // Arrange
        var family = UniqueFamily();
        var path = new SyntheticFont()
            .WithName(NameId.Family, family + " Lokalisiert", language: NameLanguage.German)
            .WithName(NameId.Subfamily, "Standard", language: NameLanguage.German)
            .WithName(NameId.Family, family)
            .WithName(NameId.Subfamily, "Regular")
            .Write(Path.Combine(_directory, "localized.ttf"));

        // Act
        FontResolver.RegisterCustomFontDirectory(_directory);
        var font = FontResolver.Resolve(family, new FontAttributes());

        // Assert
        Assert.IsNotNull(font, "The font should be resolved by its English family name.");
        Assert.AreEqual(path, font.FilePath, "The font file should be found.");
    }

    [TestMethod]
    public void Resolve_NonAsciiNameOnMacAndWindows_WindowsNameIsUsed()
    {
        // Arrange
        var family = UniqueFamily() + " Café";
        var path = new SyntheticFont()
            .WithName(NameId.Family, family, NameEncoding.MacRoman)
            .WithName(NameId.Subfamily, "Regular", NameEncoding.MacRoman)
            .WithName(NameId.Family, family)
            .WithName(NameId.Subfamily, "Regular")
            .Write(Path.Combine(_directory, "accented.ttf"));

        // Act
        FontResolver.RegisterCustomFontDirectory(_directory);
        var font = FontResolver.Resolve(family, new FontAttributes());

        // Assert
        Assert.IsNotNull(font, "The font should be resolved by the name with the accent.");
        Assert.AreEqual(path, font.FilePath, "The font file should be found.");
    }

    private static string UniqueFamily() => "Synthetic " + Guid.NewGuid().ToString("N");
}
