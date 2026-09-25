using System.ComponentModel.DataAnnotations;
using BaratoInventory.Core.DTOs;
using NUnit.Framework;

namespace BaratoInventory.Tests.Domain;

[TestFixture]
public class ValidationTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, context, validationResults, true);
        return validationResults;
    }

    [Test]
    public void ProductFormModel_WithValidProperties_PassesValidation()
    {
        // Arrange
        var model = new ProductFormModel
        {
            Name = "Valid Product Name",
            Category = "Beverages",
            Price = 45.50m,
            Quantity = 20
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        Assert.That(results, Is.Empty);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void ProductFormModel_WithMissingName_FailsValidation(string? name)
    {
        // Arrange
        var model = new ProductFormModel
        {
            Name = name!,
            Category = "Beverages",
            Price = 50.00m,
            Quantity = 10
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ProductFormModel.Name))), Is.True);
    }

    [Test]
    public void ProductFormModel_WithShortName_FailsValidation()
    {
        // Arrange
        var model = new ProductFormModel
        {
            Name = "A", // Less than 2 chars
            Category = "Beverages",
            Price = 50.00m,
            Quantity = 10
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ProductFormModel.Name))), Is.True);
    }

    [TestCase(0.00)]
    [TestCase(-10.50)]
    public void ProductFormModel_WithZeroOrNegativePrice_FailsValidation(decimal invalidPrice)
    {
        // Arrange
        var model = new ProductFormModel
        {
            Name = "Sample Product",
            Category = "Beverages",
            Price = invalidPrice,
            Quantity = 10
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ProductFormModel.Price))), Is.True);
    }

    [Test]
    public void ProductFormModel_WithNegativeQuantity_FailsValidation()
    {
        // Arrange
        var model = new ProductFormModel
        {
            Name = "Sample Product",
            Category = "Beverages",
            Price = 25.00m,
            Quantity = -5 // Invalid negative quantity
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ProductFormModel.Quantity))), Is.True);
    }

    [Test]
    public void ProductFormModel_WithValidImageUrl_PassesValidation()
    {
        // Arrange
        var model = new ProductFormModel
        {
            Name = "Sample Product",
            Category = "Beverages",
            Price = 25.00m,
            Quantity = 10,
            ImageUrl = "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=150"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void ProductFormModel_WithInvalidImageUrl_FailsValidation()
    {
        // Arrange
        var model = new ProductFormModel
        {
            Name = "Sample Product",
            Category = "Beverages",
            Price = 25.00m,
            Quantity = 10,
            ImageUrl = "not-a-valid-url"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ProductFormModel.ImageUrl))), Is.True);
    }

    [Test]
    public void ProductFormModel_WithLocalUploadedImagePath_PassesValidation()
    {
        // Arrange
        var model = new ProductFormModel
        {
            Name = "Sample Product",
            Category = "Beverages",
            Price = 25.00m,
            Quantity = 10,
            ImageUrl = "/uploads/products/fita-crackers.jpg"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        Assert.That(results, Is.Empty);
    }
}
