namespace FontResolution.Tests;

[TestClass]
public class FontNameResolverTest
{
    [TestMethod]
    public void StylizeFontNameFamily_NormalWeightAndStyle_NoWeightOrStyleIsIncluded()
    {
        // Arrange
        var metadata = new FontMetadata
        {
            FamilyName = "Arial",
            Subfamily = "Regular",
            FullName = "Arial Regular",
            PostScriptName = "ArialMT",
            PreferredFamily = "Arial",
            PreferredSubfamily = "Regular",
            Attributes = new FontAttributes
            {
                Weight = FontWeight.Normal,
                Style = FontStyle.Normal
            }
        };
        
        // Act
        var result = FontNameResolver.StylizeFontNameFamily(metadata);
        
        // Assert
        Assert.AreEqual("Arial", result);
    }
    
    [TestMethod]
    public void StylizeFontNameFamily_BoldWeightAndNormalStyle_WeightIsIncluded()
    {
        // Arrange
        var metadata = new FontMetadata
        {
            FamilyName = "Arial",
            Subfamily = "Regular",
            FullName = "Arial Regular",
            PostScriptName = "ArialMT",
            PreferredFamily = "Arial",
            PreferredSubfamily = "Regular",
            Attributes = new FontAttributes
            {
                Weight = FontWeight.Bold,
                Style = FontStyle.Normal
            }
        };
        
        // Act
        var result = FontNameResolver.StylizeFontNameFamily(metadata);
        
        // Assert
        Assert.AreEqual("Arial Bold", result);
    }
    
    [TestMethod]
    public void StylizeFontNameFamily_BoldWeightAndItalicStyle_WeightAndStyleAreIncluded()
    {
        // Arrange
        var metadata = new FontMetadata
        {
            FamilyName = "Arial",
            Subfamily = "Regular",
            FullName = "Arial Regular",
            PostScriptName = "ArialMT",
            PreferredFamily = "Arial",
            PreferredSubfamily = "Regular",
            Attributes = new FontAttributes
            {
                Weight = FontWeight.Bold,
                Style = FontStyle.Italic
            }
        };
        
        // Act
        var result = FontNameResolver.StylizeFontNameFamily(metadata);
        
        // Assert
        Assert.AreEqual("Arial Bold Italic", result);
    }
    
    [TestMethod]
    public void StylizeFontNameFamily_NormalWeightAndItalicStyle_StyleIsIncluded()
    {
        // Arrange
        var metadata = new FontMetadata
        {
            FamilyName = "Arial",
            Subfamily = "Regular",
            FullName = "Arial Regular",
            PostScriptName = "ArialMT",
            PreferredFamily = "Arial",
            PreferredSubfamily = "Regular",
            Attributes = new FontAttributes
            {
                Weight = FontWeight.Normal,
                Style = FontStyle.Italic
            }
        };
        
        // Act
        var result = FontNameResolver.StylizeFontNameFamily(metadata);
        
        // Assert
        Assert.AreEqual("Arial Italic", result);
    }
    
    [TestMethod]
    public void StylizeFontNamePreferredFamily_BlackWeightAndNormalStyle_WeightAndStyleAreNotIncluded()
    {
        // Arrange
        var metadata = new FontMetadata
        {
            FamilyName = "Arial Black",
            Subfamily = "Regular",
            FullName = "Arial Black",
            PostScriptName = "Arial-Black",
            PreferredFamily = "Arial",
            PreferredSubfamily = "Black",
            Attributes = new FontAttributes
            {
                Weight = FontWeight.Black,
                Style = FontStyle.Normal
            }
        };
        
        // Act
        var result = FontNameResolver.StylizeFontNamePreferredFamily(metadata);
        
        // Assert
        Assert.AreEqual("Arial Black", result);
    }
    
    [TestMethod]
    public void StylizeFontNamePreferredFamily_NormalWeightAndNormalStyle_WeightAndStyleAreNotIncluded()
    {
        // Arrange
        var metadata = new FontMetadata
        {
            FamilyName = "Arial Black",
            Subfamily = "Regular",
            FullName = "Arial Black",
            PostScriptName = "Arial-Black",
            PreferredFamily = "Arial",
            PreferredSubfamily = "Black",
            Attributes = new FontAttributes
            {
                Weight = FontWeight.Normal,
                Style = FontStyle.Normal
            }
        };
        
        // Act
        var result = FontNameResolver.StylizeFontNamePreferredFamily(metadata);
        
        // Assert
        Assert.AreEqual("Arial Black", result);
    }
    
    [TestMethod]
    public void StylizeFontNameStrict_NormalWeightAndNormalStyle_WeightAndStyleAreNotIncluded()
    {
        // Arrange
        var fontName = "Arial";
        var fontAttributes = new FontAttributes
        {
            Weight = FontWeight.Normal,
            Style = FontStyle.Normal
        };
        
        // Act
        var result = FontNameResolver.StylizeFontNameStrict(fontName, fontAttributes);
        
        // Assert
        Assert.AreEqual("Arial", result);
    }
    
    [TestMethod]
    public void StylizeFontNameStrict_NormalWeightAndObliqueStyle_StyleIsIncluded()
    {
        // Arrange
        var fontName = "Arial";
        var fontAttributes = new FontAttributes
        {
            Weight = FontWeight.Normal,
            Style = FontStyle.Oblique
        };
        
        // Act
        var result = FontNameResolver.StylizeFontNameStrict(fontName, fontAttributes);
        
        // Assert
        Assert.AreEqual("Arial Oblique", result);
    }
    
    [TestMethod]
    public void StylizeFontNameStrict_BoldWeightAndItalicStyle_WeightAndStyleAreIncluded()
    {
        // Arrange
        var fontName = "Arial";
        var fontAttributes = new FontAttributes
        {
            Weight = FontWeight.Bold,
            Style = FontStyle.Italic
        };
        
        // Act
        var result = FontNameResolver.StylizeFontNameStrict(fontName, fontAttributes);
        
        // Assert
        Assert.AreEqual("Arial Bold Italic", result);
    }

    [TestMethod]
    public void StylizeFontNameClosest_NormalWeightAndNormalStyle_OneResultIsReturned()
    {
        // Arrange
        var fontName = "Arial";
        var fontAttributes = new FontAttributes
        {
            Weight = FontWeight.Normal,
            Style = FontStyle.Normal
        };
        
        // Act
        var result = FontNameResolver.StylizeFontNameClosest(fontName, fontAttributes);
        
        // Assert
        Assert.ContainsSingle(result);
        Assert.AreEqual("Arial", result[0]);
    }

    [TestMethod]
    public void StylizeFontNameClosest_NormalWeightAndObliqueStyle_ListOfClosestFontNamesIsReturned()
    {
        // Arrange
        var fontName = "Arial";
        var fontAttributes = new FontAttributes
        {
            Weight = FontWeight.Normal,
            Style = FontStyle.Oblique
        };
        
        // Act
        var result = FontNameResolver.StylizeFontNameClosest(fontName, fontAttributes);
        
        // Assert
        Assert.HasCount(3, result);
        Assert.AreEqual("Arial Oblique", result[0]);
        Assert.AreEqual("Arial Italic", result[1]);
        Assert.AreEqual("Arial", result[2]);
    }

    [TestMethod]
    public void StylizeFontNameClosest_NormalWeightAndItalicStyle_ListOfClosestFontNamesIsReturned()
    {
        // Arrange
        var fontName = "Arial";
        var fontAttributes = new FontAttributes
        {
            Weight = FontWeight.Normal,
            Style = FontStyle.Italic
        };
        
        // Act
        var result = FontNameResolver.StylizeFontNameClosest(fontName, fontAttributes);
        
        // Assert
        Assert.HasCount(2, result);
        Assert.AreEqual("Arial Italic", result[0]);
        Assert.AreEqual("Arial", result[1]);
    }

    [TestMethod]
    public void StylizeFontNameClosest_LightWeightAndObliqueStyle_ListOfClosestFontNamesIsReturned()
    {
        // Arrange
        var fontName = "Arial";
        var fontAttributes = new FontAttributes
        {
            Weight = FontWeight.Light,
            Style = FontStyle.Oblique
        };
        
        // Act
        var result = FontNameResolver.StylizeFontNameClosest(fontName, fontAttributes);
        
        // Assert
        Assert.HasCount(5, result);
        Assert.AreEqual("Arial Light Oblique", result[0]);
        Assert.AreEqual("Arial Light Italic", result[1]);
        Assert.AreEqual("Arial ExtraLight Oblique", result[2]);
        Assert.AreEqual("Arial ExtraLight Italic", result[3]);
        Assert.AreEqual("Arial", result[4]);
    }
}