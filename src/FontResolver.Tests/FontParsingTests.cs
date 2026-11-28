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

    [TestMethod]
    [DataRow(NameEncoding.WindowsSymbol)]
    [DataRow(NameEncoding.UnicodeBmp)]
    public void Resolve_NamesInUtf16Encodings_FamilyIsResolved(NameEncoding encoding)
    {
        // Arrange
        var family = UniqueFamily();
        var path = new SyntheticFont()
            .WithName(NameId.Family, family, encoding)
            .WithName(NameId.Subfamily, "Regular", encoding)
            .Write(Path.Combine(_directory, "encoding.ttf"));

        // Act
        FontResolver.RegisterCustomFontDirectory(_directory);
        var font = FontResolver.Resolve(family, new FontAttributes());

        // Assert
        Assert.IsNotNull(font, $"The font with {encoding} names should be resolved.");
        Assert.AreEqual(path, font.FilePath, "The font file should be found.");
    }

    [TestMethod]
    [DataRow("SemiCondensed", FontWeight.Normal, FontWidth.SemiCondensed)]
    [DataRow("SemiExpanded", FontWeight.Normal, FontWidth.SemiExpanded)]
    [DataRow("Extra Bold", FontWeight.ExtraBold, FontWidth.Medium)]
    [DataRow("UltraLight", FontWeight.ExtraLight, FontWidth.Medium)]
    public void Resolve_FontWithoutOs2Table_AttributesAreTakenFromTheSubfamily(
        string subfamily,
        FontWeight weight,
        FontWidth width
    )
    {
        // Arrange
        var family = UniqueFamily();
        new SyntheticFont()
            .WithName(NameId.Family, family)
            .WithName(NameId.Subfamily, subfamily)
            .Write(Path.Combine(_directory, "subfamily.ttf"));

        // Act
        FontResolver.RegisterCustomFontDirectory(_directory);
        var font = FontResolver.Resolve(family, new FontAttributes(weight, FontStyle.Normal, width));

        // Assert
        Assert.IsNotNull(font, $"The {subfamily} face should be resolved with weight {weight} and width {width}.");
    }

    [TestMethod]
    public void Resolve_ClosestWithExtraWeightsInTheFamily_RegularFaceIsResolved()
    {
        // Arrange
        var family = UniqueFamily();
        var regularPath = new SyntheticFont()
            .WithName(NameId.Family, family)
            .WithName(NameId.Subfamily, "Regular")
            .WithOs2(FontWeight.Normal)
            .Write(Path.Combine(_directory, "regular.ttf"));
        new SyntheticFont()
            .WithName(NameId.Family, family + " Light")
            .WithName(NameId.Subfamily, "Regular")
            .WithName(NameId.TypographicFamily, family)
            .WithName(NameId.TypographicSubfamily, "Light")
            .WithOs2(FontWeight.Light)
            .Write(Path.Combine(_directory, "light.ttf"));

        // Act
        FontResolver.RegisterCustomFontDirectory(_directory);
        var font = FontResolver.Resolve(family, new FontAttributes(), FontResolveStrategy.Closest);

        // Assert
        Assert.IsNotNull(font, "The font should be resolved.");
        Assert.AreEqual(regularPath, font.FilePath, "The regular face should match normal attributes best.");
    }

    [TestMethod]
    public void Resolve_FontFileWithUpperCaseExtension_FontIsResolved()
    {
        // Arrange
        var family = UniqueFamily();
        var path = new SyntheticFont()
            .WithName(NameId.Family, family)
            .WithName(NameId.Subfamily, "Regular")
            .Write(Path.Combine(_directory, "upper.TTF"));

        // Act
        FontResolver.RegisterCustomFontDirectory(_directory);
        var font = FontResolver.Resolve(family, new FontAttributes());

        // Assert
        Assert.IsNotNull(font, "A font file with an upper case extension should be found.");
        Assert.AreEqual(path, font.FilePath, "The font file should be found.");
    }

    private static string UniqueFamily() => "Synthetic " + Guid.NewGuid().ToString("N");
}
