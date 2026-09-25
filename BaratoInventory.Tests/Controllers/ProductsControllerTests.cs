using BaratoInventory.API.Controllers;
using BaratoInventory.Core.DTOs;
using BaratoInventory.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace BaratoInventory.Tests.Controllers;

[TestFixture]
public class ProductsControllerTests
{
    private Mock<IProductService> _serviceMock = null!;
    private ProductsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IProductService>();
        _controller = new ProductsController(_serviceMock.Object);
    }

    [Test]
    public async Task GetProducts_ReturnsOkResultWithPagedResult()
    {
        // Arrange
        var pagedResult = new PagedResult<ProductDto>(
            Items: new List<ProductDto>
            {
                new(1, "Test Prod", "Beverages", 50.00m, 10, DateTime.UtcNow, null, [])
            },
            TotalCount: 1,
            PageNumber: 1,
            PageSize: 10,
            TotalPages: 1
        );

        _serviceMock.Setup(s => s.GetProductsAsync(null, null, null, false, 1, 10, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetProducts(null, null, null, false, 1, 10);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(pagedResult));
    }

    [Test]
    public async Task GetProduct_WhenProductExists_ReturnsOkResult()
    {
        // Arrange
        var product = new ProductDto(1, "Test Prod", "Beverages", 50.00m, 10, DateTime.UtcNow, null, []);
        _serviceMock.Setup(s => s.GetProductByIdAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(product);

        // Act
        var result = await _controller.GetProduct(1, CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(product));
    }

    [Test]
    public async Task GetProduct_WhenProductNotFound_ReturnsNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetProductByIdAsync(999, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ProductDto?)null);

        // Act
        var result = await _controller.GetProduct(999, CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task CreateProduct_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateProductDto("New Prod", "Beverages", 35.00m, 20);
        var createdDto = new ProductDto(10, "New Prod", "Beverages", 35.00m, 20, DateTime.UtcNow, null, []);

        _serviceMock.Setup(s => s.CreateProductAsync(createDto, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(createdDto);

        // Act
        var result = await _controller.CreateProduct(createDto, CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = result as CreatedAtActionResult;
        Assert.That(createdResult!.Value, Is.EqualTo(createdDto));
    }

    [Test]
    public async Task UpdateProduct_ReturnsNoContent()
    {
        // Arrange
        var updateDto = new UpdateProductDto("Updated Prod", "Beverages", 40.00m, 25, []);

        // Act
        var result = await _controller.UpdateProduct(1, updateDto, CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
        _serviceMock.Verify(s => s.UpdateProductAsync(1, updateDto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task AdjustQuantity_ReturnsOk()
    {
        // Arrange
        var adjustDto = new AdjustQuantityDto(5, []);

        // Act
        var result = await _controller.AdjustQuantity(1, adjustDto, CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<OkResult>());
        _serviceMock.Verify(s => s.AdjustQuantityAsync(1, adjustDto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteProduct_ReturnsNoContent()
    {
        // Act
        var result = await _controller.DeleteProduct(1, CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
        _serviceMock.Verify(s => s.DeleteProductAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetCategories_ReturnsOkWithList()
    {
        // Arrange
        var categories = new List<string> { "Bakery", "Beverages", "Dairy" };
        _serviceMock.Setup(s => s.GetCategoriesAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(categories);

        // Act
        var result = await _controller.GetCategories(CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(categories));
    }

    [TestCase(0)]
    [TestCase(-5)]
    public void CreateProductDto_WhenQuantityIsZeroOrNegative_FailsValidation(int invalidQuantity)
    {
        // Arrange
        var dto = new CreateProductDto("Test Product", "Bakery", 10.00m, invalidQuantity);
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(dto);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        // Act
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(dto, validationContext, validationResults, true);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(validationResults.Any(v => v.MemberNames.Contains("Quantity")), Is.True);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void ProductFormModel_InAddMode_WhenQuantityIsZeroOrNegative_FailsValidation(int invalidQuantity)
    {
        // Arrange
        var model = new ProductFormModel
        {
            Id = 0,
            Name = "Test Product",
            Category = "Bakery",
            Price = 10.00m,
            Quantity = invalidQuantity
        };
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(model);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        // Act
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(model, validationContext, validationResults, true);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(model.HasValidQuantity, Is.False);
        Assert.That(validationResults.Any(v => v.ErrorMessage!.Contains("Initial quantity must be greater than zero")), Is.True);
    }
}

