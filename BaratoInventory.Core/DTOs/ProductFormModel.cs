using System.ComponentModel.DataAnnotations;

namespace BaratoInventory.Core.DTOs;

public class ProductFormModel : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 150 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category must be between 2 and 100 characters.")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, 999999.99, ErrorMessage = "Price must be a positive amount greater than ₱0.00.")]
    public decimal Price { get; set; } = 1.00m;

    [Required(ErrorMessage = "Quantity is required.")]
    public int Quantity { get; set; } = 1;

    [MaxLength(500, ErrorMessage = "Image URL must not exceed 500 characters.")]
    public string? ImageUrl { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public bool IsEditMode => Id > 0;

    public bool HasValidQuantity => IsEditMode ? Quantity >= 0 : Quantity > 0;

    public static ProductFormModel FromDto(ProductDto dto)
    {
        return new ProductFormModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Category = dto.Category,
            Price = dto.Price,
            Quantity = dto.Quantity,
            ImageUrl = dto.ImageUrl,
            RowVersion = dto.RowVersion
        };
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsEditMode && Quantity <= 0)
        {
            yield return new ValidationResult(
                "Initial quantity must be greater than zero.",
                [nameof(Quantity)]);
        }
        else if (Quantity < 0)
        {
            yield return new ValidationResult(
                "Quantity cannot be negative.",
                [nameof(Quantity)]);
        }

        if (!string.IsNullOrWhiteSpace(ImageUrl))
        {
            var trimmed = ImageUrl.Trim();
            bool isAbsoluteHttp = Uri.TryCreate(trimmed, UriKind.Absolute, out var uriResult)
                                  && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            bool isRelativePath = trimmed.StartsWith('/') || trimmed.StartsWith("./");

            if (!isAbsoluteHttp && !isRelativePath)
            {
                yield return new ValidationResult(
                    "Please enter a valid web image URL (http:// or https://) or an uploaded image path.",
                    [nameof(ImageUrl)]);
            }
        }
    }

    public CreateProductDto ToCreateDto() => new(Name.Trim(), Category.Trim(), Price, Quantity, string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl.Trim());

    public UpdateProductDto ToUpdateDto() => new(Name.Trim(), Category.Trim(), Price, Quantity, RowVersion, string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl.Trim());
}
