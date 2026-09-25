using BaratoInventory.Core.DTOs;
using BaratoInventory.Core.Entities;
using BaratoInventory.Core.Exceptions;
using BaratoInventory.Core.Interfaces;
using BaratoInventory.Infrastructure.Data;
using BaratoInventory.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace BaratoInventory.Tests.Services;

[TestFixture]
public class ProductServiceTests
{
    private AppDbContext _context = null!;
    private Mock<ICacheService> _cacheMock = null!;
    private ProductService _service = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _cacheMock = new Mock<ICacheService>();
        _service = new ProductService(_context, _cacheMock.Object);

        // Seed 3 test products
        _context.Products.AddRange(
            new Product { Id = 1, Name = "Apple Juice 1L", Category = "Beverages", Price = 80.00m, Quantity = 50, CreatedAtUtc = DateTime.UtcNow },
            new Product { Id = 2, Name = "White Bread 500g", Category = "Bakery", Price = 60.00m, Quantity = 25, CreatedAtUtc = DateTime.UtcNow },
            new Product { Id = 3, Name = "Cheddar Cheese 200g", Category = "Dairy", Price = 120.00m, Quantity = 5, CreatedAtUtc = DateTime.UtcNow }
        );
        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task GetProductsAsync_WhenCached_ReturnsCachedResultWithoutDbQuery()
    {
        // Arrange
        var cachedItems = new List<ProductDto>
        {
            new(99, "Cached Item", "Snacks", 99.00m, 10, DateTime.UtcNow, null, [])
        };
        var cachedEnvelope = new PagedResult<ProductDto>(cachedItems, 1, 1, 10, 1);

        _cacheMock.Setup(c => c.GetAsync<PagedResult<ProductDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(cachedEnvelope);

        // Act
        var result = await _service.GetProductsAsync(null, null, null, false, 1, 10, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.First().Name, Is.EqualTo("Cached Item"));
        _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<PagedResult<ProductDto>>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetProductsAsync_WhenNotCached_QueriesDbAndSetsCache()
    {
        // Arrange
        _cacheMock.Setup(c => c.GetAsync<PagedResult<ProductDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((PagedResult<ProductDto>?)null);

        // Act
        var result = await _service.GetProductsAsync(null, null, null, false, 1, 10, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.TotalCount, Is.EqualTo(3));
        _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<PagedResult<ProductDto>>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateProductAsync_PersistsProductAndInvalidatesCache()
    {
        // Arrange
        var newProductDto = new CreateProductDto("Instant Coffee 100g", "Beverages", 110.00m, 30);

        // Act
        var created = await _service.CreateProductAsync(newProductDto, CancellationToken.None);

        // Assert
        Assert.That(created.Id, Is.GreaterThan(0));
        var inDb = await _context.Products.FindAsync(created.Id);
        Assert.That(inDb, Is.Not.Null);
        Assert.That(inDb!.Name, Is.EqualTo("Instant Coffee 100g"));

        _cacheMock.Verify(c => c.RemoveByPrefixAsync("barato:products:list", It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync("barato:categories:all", It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-100)]
    public void CreateProductAsync_WhenQuantityIsZeroOrNegative_ThrowsArgumentException(int invalidQuantity)
    {
        // Arrange
        var invalidDto = new CreateProductDto("Invalid Product", "Bakery", 50.00m, invalidQuantity);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _service.CreateProductAsync(invalidDto, CancellationToken.None);
        });

        Assert.That(ex!.Message, Does.Contain("Initial quantity must be greater than zero"));
    }

    [Test]
    public async Task AdjustQuantityAsync_WithValidStock_UpdatesQuantitySuccessfully()
    {
        // Arrange
        var dto = new AdjustQuantityDto(QuantityAdjustment: 10, RowVersion: []);

        // Act
        await _service.AdjustQuantityAsync(1, dto, CancellationToken.None);

        // Assert
        var updated = await _context.Products.FindAsync(1);
        Assert.That(updated!.Quantity, Is.EqualTo(60)); // 50 + 10 = 60
        _cacheMock.Verify(c => c.RemoveAsync("barato:products:item:1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void AdjustQuantityAsync_WhenReducingBelowZero_ThrowsInvalidOperationException()
    {
        // Arrange: Product 3 only has 5 units
        var dto = new AdjustQuantityDto(QuantityAdjustment: -10, RowVersion: []);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _service.AdjustQuantityAsync(3, dto, CancellationToken.None);
        });

        Assert.That(ex!.Message, Does.Contain("Insufficient stock"));
    }

    [Test]
    public async Task DeleteProductAsync_PerformsSoftDeleteAndInvalidatesCache()
    {
        // Act
        await _service.DeleteProductAsync(2, CancellationToken.None);

        // Assert: Product 2 should have IsDeleted = true
        var product = await _context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == 2);
        Assert.That(product, Is.Not.Null);
        Assert.That(product!.IsDeleted, Is.True);

        _cacheMock.Verify(c => c.RemoveAsync("barato:products:item:2", It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveByPrefixAsync("barato:products:list", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateProductAsync_WithValidDto_UpdatesProductAndInvalidatesCache()
    {
        // Arrange
        var updateDto = new UpdateProductDto("Coke 2.0L", "Beverages", 80.00m, 120, []);

        // Act
        await _service.UpdateProductAsync(1, updateDto, CancellationToken.None);

        // Assert
        var updated = await _context.Products.FindAsync(1);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Name, Is.EqualTo("Coke 2.0L"));
        Assert.That(updated.Price, Is.EqualTo(80.00m));
        Assert.That(updated.Quantity, Is.EqualTo(120));

        _cacheMock.Verify(c => c.RemoveByPrefixAsync("barato:products:list", It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync("barato:products:item:1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateProductAsync_WithEmptyRowVersion_DoesNotThrowAndUpdatesSuccessfully()
    {
        // Arrange
        var updateDto = new UpdateProductDto("Gardenia Wheat Bread", "Bakery", 85.00m, 40, []);

        // Act & Assert
        Assert.DoesNotThrowAsync(async () =>
        {
            await _service.UpdateProductAsync(2, updateDto, CancellationToken.None);
        });

        var updated = await _context.Products.FindAsync(2);
        Assert.That(updated!.Quantity, Is.EqualTo(40));
    }

    [Test]
    public void UpdateProductAsync_WhenProductNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var updateDto = new UpdateProductDto("Nonexistent", "General", 10.00m, 5, []);

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await _service.UpdateProductAsync(9999, updateDto, CancellationToken.None);
        });
    }
}
