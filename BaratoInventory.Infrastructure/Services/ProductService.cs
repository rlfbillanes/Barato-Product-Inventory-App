using BaratoInventory.Core.DTOs;
using BaratoInventory.Core.Entities;
using BaratoInventory.Core.Exceptions;
using BaratoInventory.Core.Interfaces;
using BaratoInventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BaratoInventory.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly ICacheService _cache;

    public ProductService(AppDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(string? search, string? category, string? sortBy, bool descending, int page, int pageSize, CancellationToken cancellationToken)
    {
        string cacheKey = $"barato:products:list:p{page}:s{pageSize}:search{search}:cat{category}:sort{sortBy}_{descending}";
        var cachedResult = await _cache.GetAsync<PagedResult<ProductDto>>(cacheKey, cancellationToken);
        if (cachedResult != null) return cachedResult;

        var query = _context.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortBy?.ToLower() switch
        {
            "name" => descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => descending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "quantity" => descending ? query.OrderByDescending(p => p.Quantity) : query.OrderBy(p => p.Quantity),
            "category" => descending ? query.OrderByDescending(p => p.Category) : query.OrderBy(p => p.Category),
            _ => descending ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto(p.Id, p.Name, p.Category, p.Price, p.Quantity, p.CreatedAtUtc, p.UpdatedAtUtc, p.RowVersion, p.ImageUrl))
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var result = new PagedResult<ProductDto>(items, totalCount, page, pageSize, totalPages);

        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10), cancellationToken);
        return result;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken)
    {
        string cacheKey = $"barato:products:item:{id}";
        var cached = await _cache.GetAsync<ProductDto>(cacheKey, cancellationToken);
        if (cached != null) return cached;

        var p = await _context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (p == null) return null;

        var dto = new ProductDto(p.Id, p.Name, p.Category, p.Price, p.Quantity, p.CreatedAtUtc, p.UpdatedAtUtc, p.RowVersion, p.ImageUrl);
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(15), cancellationToken);
        return dto;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken)
    {
        if (dto.Quantity <= 0)
        {
            throw new ArgumentException("Initial quantity must be greater than zero.");
        }

        var product = new Product
        {
            Name = dto.Name,
            Category = dto.Category,
            Price = dto.Price,
            Quantity = dto.Quantity,
            ImageUrl = dto.ImageUrl
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        await InvalidateCachesAsync();

        return new ProductDto(product.Id, product.Name, product.Category, product.Price, product.Quantity, product.CreatedAtUtc, product.UpdatedAtUtc, product.RowVersion, product.ImageUrl);
    }

    public async Task UpdateProductAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync([id], cancellationToken);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {id} was not found.");

        product.Name = dto.Name;
        product.Category = dto.Category;
        product.Price = dto.Price;
        product.Quantity = dto.Quantity;
        product.ImageUrl = dto.ImageUrl;
        
        if (dto.RowVersion != null && dto.RowVersion.Length > 0)
        {
            _context.Entry(product).Property(p => p.RowVersion).OriginalValue = dto.RowVersion;
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await InvalidateCachesAsync(id);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("The product was modified by another user or session concurrently. Please reload the latest data and try again.", ex);
        }
    }

    public async Task DeleteProductAsync(int id, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync([id], cancellationToken);
        if (product == null) return;

        product.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        await InvalidateCachesAsync(id);
    }

    public async Task AdjustQuantityAsync(int id, AdjustQuantityDto dto, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync([id], cancellationToken);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {id} was not found.");

        if (product.Quantity + dto.QuantityAdjustment < 0)
            throw new InvalidOperationException("Insufficient stock to complete this adjustment.");

        product.Quantity += dto.QuantityAdjustment;
        if (dto.RowVersion != null && dto.RowVersion.Length > 0)
        {
            _context.Entry(product).Property(p => p.RowVersion).OriginalValue = dto.RowVersion;
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await InvalidateCachesAsync(id);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("The product was modified by another user or session concurrently. Please reload the latest data and try again.", ex);
        }
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        string cacheKey = "barato:categories:all";
        var cached = await _cache.GetAsync<IEnumerable<string>>(cacheKey, cancellationToken);
        if (cached != null) return cached;

        var categories = await _context.Products
            .AsNoTracking()
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);

        await _cache.SetAsync(cacheKey, categories, TimeSpan.FromMinutes(60), cancellationToken);
        return categories;
    }

    private async Task InvalidateCachesAsync(int? id = null)
    {
        await _cache.RemoveByPrefixAsync("barato:products:list");
        await _cache.RemoveAsync("barato:categories:all");
        if (id.HasValue)
        {
            await _cache.RemoveAsync($"barato:products:item:{id.Value}");
        }
    }
}
