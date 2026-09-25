using System.ComponentModel.DataAnnotations;

namespace BaratoInventory.Core.DTOs;

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

public record ProductDto(
    int Id,
    string Name,
    string Category,
    decimal Price,
    int Quantity,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    byte[] RowVersion,
    string? ImageUrl = null
);

public record CreateProductDto
{
    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 150 characters.")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Category is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category must be between 2 and 100 characters.")]
    public string Category { get; init; } = string.Empty;

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, 999999.99, ErrorMessage = "Price must be a positive amount.")]
    public decimal Price { get; init; }

    [Required(ErrorMessage = "Initial quantity is required.")]
    [Range(1, 1000000, ErrorMessage = "Initial quantity must be greater than zero.")]
    public int Quantity { get; init; }

    [StringLength(500, ErrorMessage = "Image URL must not exceed 500 characters.")]
    public string? ImageUrl { get; init; }

    public CreateProductDto() { }

    public CreateProductDto(string name, string category, decimal price, int quantity, string? imageUrl = null)
    {
        Name = name;
        Category = category;
        Price = price;
        Quantity = quantity;
        ImageUrl = imageUrl;
    }
}

public record UpdateProductDto
{
    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 150 characters.")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Category is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category must be between 2 and 100 characters.")]
    public string Category { get; init; } = string.Empty;

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, 999999.99, ErrorMessage = "Price must be a positive amount.")]
    public decimal Price { get; init; }

    [Required(ErrorMessage = "Quantity is required.")]
    [Range(0, 1000000, ErrorMessage = "Quantity cannot be negative.")]
    public int Quantity { get; init; }

    public byte[]? RowVersion { get; init; }

    [StringLength(500, ErrorMessage = "Image URL must not exceed 500 characters.")]
    public string? ImageUrl { get; init; }

    public UpdateProductDto() { }

    public UpdateProductDto(string name, string category, decimal price, int quantity, byte[]? rowVersion = null, string? imageUrl = null)
    {
        Name = name;
        Category = category;
        Price = price;
        Quantity = quantity;
        RowVersion = rowVersion;
        ImageUrl = imageUrl;
    }
}

public record AdjustQuantityDto(
    int QuantityAdjustment,
    byte[]? RowVersion = null
);

public record UploadResultDto(
    string RelativeUrl,
    string FullUrl,
    string FileName,
    long FileSize
);

public record ApiResult(
    bool IsSuccess,
    int StatusCode,
    string? ErrorMessage = null,
    bool IsConcurrencyConflict = false
);
