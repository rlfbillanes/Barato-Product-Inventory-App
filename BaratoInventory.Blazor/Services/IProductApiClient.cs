using BaratoInventory.Core.DTOs;

namespace BaratoInventory.Blazor.Services;

public interface IProductApiClient
{
    Task<PagedResult<ProductDto>?> GetProductsAsync(
        string? search = null, 
        string? category = null, 
        string? sortBy = null, 
        bool descending = false, 
        int page = 1, 
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ProductDto?> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default);

    Task<bool> UpdateProductAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default);

    Task<ApiResult> UpdateProductWithResultAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default);

    Task<bool> AdjustQuantityAsync(int id, AdjustQuantityDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    Task<UploadResultDto?> UploadImageAsync(Microsoft.AspNetCore.Components.Forms.IBrowserFile file, CancellationToken cancellationToken = default);

    string ResolveImageUrl(string? imageUrl);
}
