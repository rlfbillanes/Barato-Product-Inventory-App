using BaratoInventory.Core.DTOs;

namespace BaratoInventory.Core.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetProductsAsync(string? search, string? category, string? sortBy, bool descending, int page, int pageSize, CancellationToken cancellationToken);
    Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken);
    Task UpdateProductAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken);
    Task DeleteProductAsync(int id, CancellationToken cancellationToken);
    Task AdjustQuantityAsync(int id, AdjustQuantityDto dto, CancellationToken cancellationToken);
    Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken);
}
