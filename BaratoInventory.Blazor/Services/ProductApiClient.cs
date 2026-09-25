using System.Net.Http.Json;
using BaratoInventory.Core.DTOs;

namespace BaratoInventory.Blazor.Services;

public class ProductApiClient : IProductApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ProductApiClient> _logger;

    public ProductApiClient(HttpClient http, ILogger<ProductApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<PagedResult<ProductDto>?> GetProductsAsync(
        string? search = null,
        string? category = null,
        string? sortBy = null,
        bool descending = false,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}",
                $"descending={descending.ToString().ToLower()}"
            };

            if (!string.IsNullOrWhiteSpace(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");

            if (!string.IsNullOrWhiteSpace(category) && category != "All Categories")
                queryParams.Add($"category={Uri.EscapeDataString(category)}");

            if (!string.IsNullOrWhiteSpace(sortBy))
                queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");

            var url = $"api/products?{string.Join("&", queryParams)}";
            return await _http.GetFromJsonAsync<PagedResult<ProductDto>>(url, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve products from API.");
            return null;
        }
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<ProductDto>($"api/products/{id}", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve product {Id} from API.", id);
            return null;
        }
    }

    public async Task<ProductDto?> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/products", dto, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken: cancellationToken);
            }

            _logger.LogWarning("CreateProduct returned {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while creating product.");
            return null;
        }
    }

    public async Task<bool> UpdateProductAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var result = await UpdateProductWithResultAsync(id, dto, cancellationToken);
        return result.IsSuccess;
    }

    public async Task<ApiResult> UpdateProductWithResultAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/products/{id}", dto, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return new ApiResult(true, (int)response.StatusCode);
            }

            var statusCode = (int)response.StatusCode;
            string? errorMessage = null;

            try
            {
                var contentString = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = System.Text.Json.JsonDocument.Parse(contentString);
                var root = doc.RootElement;
                if (root.TryGetProperty("detail", out var detailElem) && detailElem.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    errorMessage = detailElem.GetString();
                }
                else if (root.TryGetProperty("title", out var titleElem) && titleElem.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    errorMessage = titleElem.GetString();
                }
            }
            catch
            {
                // Ignore parse errors and use fallback
            }

            bool isConflict = statusCode == 409 
                || (errorMessage?.Contains("modified by another user", StringComparison.OrdinalIgnoreCase) ?? false)
                || (errorMessage?.Contains("concurrently", StringComparison.OrdinalIgnoreCase) ?? false);

            return new ApiResult(false, statusCode, errorMessage ?? "Failed to update product.", isConflict);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while updating product {Id}.", id);
            return new ApiResult(false, 500, ex.Message);
        }
    }

    public async Task<bool> AdjustQuantityAsync(int id, AdjustQuantityDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _http.PatchAsJsonAsync($"api/products/{id}/quantity", dto, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while adjusting quantity for product {Id}.", id);
            return false;
        }
    }

    public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/products/{id}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while deleting product {Id}.", id);
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<IEnumerable<string>>("api/products/categories", cancellationToken);
            return result ?? Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while retrieving categories.");
            return Enumerable.Empty<string>();
        }
    }

    public async Task<UploadResultDto?> UploadImageAsync(Microsoft.AspNetCore.Components.Forms.IBrowserFile file, CancellationToken cancellationToken = default)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            // Allow up to 2 MB reading stream
            var fileStream = file.OpenReadStream(maxAllowedSize: 2 * 1024 * 1024);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
            content.Add(fileContent, "file", file.Name);

            var response = await _http.PostAsync("api/products/upload-image", content, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UploadResultDto>(cancellationToken: cancellationToken);
            }

            _logger.LogWarning("UploadImage returned status code {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while uploading product image file.");
            return null;
        }
    }

    public string ResolveImageUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return string.Empty;

        if (imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
            imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            imageUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return imageUrl;
        }

        var baseUri = _http.BaseAddress?.ToString().TrimEnd('/') ?? "";
        var relative = imageUrl.TrimStart('/');
        return $"{baseUri}/{relative}";
    }
}
