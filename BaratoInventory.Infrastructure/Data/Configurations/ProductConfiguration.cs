using BaratoInventory.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaratoInventory.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);
            
        builder.Property(x => x.Category)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500)
            .IsRequired(false);
            
        // Concurrency token
        builder.Property(x => x.RowVersion)
            .IsRowVersion();
            
        // Global Query Filter for soft delete
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Indexes
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.Category);
        
        // Seed data
        var now = new DateTime(2026, 9, 24, 0, 0, 0, DateTimeKind.Utc);
        
        builder.HasData(
            new Product { Id = 1, Name = "Coke 1.5L", Category = "Beverages", Price = 65.00m, Quantity = 100, ImageUrl = "/uploads/products/coke-1-5l.jpg", CreatedAtUtc = now },
            new Product { Id = 2, Name = "Gardenia Classic White Bread", Category = "Bakery", Price = 75.00m, Quantity = 50, ImageUrl = "/uploads/products/gardenia-bread.png", CreatedAtUtc = now },
            new Product { Id = 3, Name = "Alaska Fresh Milk 1L", Category = "Dairy", Price = 95.00m, Quantity = 30, ImageUrl = "/uploads/products/alaska-milk.jpg", CreatedAtUtc = now },
            new Product { Id = 4, Name = "Century Tuna Flakes in Oil", Category = "Canned Goods", Price = 35.00m, Quantity = 200, ImageUrl = "/uploads/products/century-tuna.jpg", CreatedAtUtc = now },
            new Product { Id = 5, Name = "Piattos Cheese 85g", Category = "Snacks", Price = 38.00m, Quantity = 150, ImageUrl = "/uploads/products/piattos-cheese.jpg", CreatedAtUtc = now },
            new Product { Id = 6, Name = "Ariel Powder 1kg", Category = "Household", Price = 250.00m, Quantity = 45, ImageUrl = "/uploads/products/ariel-powder.jpg", CreatedAtUtc = now },
            new Product { Id = 7, Name = "Joy Dishwashing Liquid 500ml", Category = "Household", Price = 110.00m, Quantity = 80, ImageUrl = "/uploads/products/joy-dishwashing.jpg", CreatedAtUtc = now },
            new Product { Id = 8, Name = "Nescafé Classic 200g", Category = "Beverages", Price = 155.00m, Quantity = 120, ImageUrl = "/uploads/products/nescafe-classic.jpg", CreatedAtUtc = now },
            new Product { Id = 9, Name = "Purefoods Tender Juicy Hotdog 1kg", Category = "Frozen", Price = 320.00m, Quantity = 60, ImageUrl = "/uploads/products/purefoods-hotdog.jpg", CreatedAtUtc = now },
            new Product { Id = 10, Name = "Datu Puti Soy Sauce 1L", Category = "Condiments", Price = 45.00m, Quantity = 300, ImageUrl = "/uploads/products/datu-puti-soysauce.jpg", CreatedAtUtc = now },
            new Product { Id = 11, Name = "Mang Tomas Sarsa 330g", Category = "Condiments", Price = 55.00m, Quantity = 140, ImageUrl = "/uploads/products/mang-tomas-sarsa.jpg", CreatedAtUtc = now },
            new Product { Id = 12, Name = "Ligo Sardines in Tomato Sauce 155g", Category = "Canned Goods", Price = 22.00m, Quantity = 500, ImageUrl = "/uploads/products/ligo-sardines.webp", CreatedAtUtc = now },
            new Product { Id = 13, Name = "Kopiko Blanca Twin Pack", Category = "Beverages", Price = 15.00m, Quantity = 400, ImageUrl = "/uploads/products/kopiko-blanca.jpg", CreatedAtUtc = now },
            new Product { Id = 14, Name = "Lucky Me! Pancit Canton Kalamansi", Category = "Noodles", Price = 18.00m, Quantity = 600, ImageUrl = "/uploads/products/lucky-me-kalamansi.webp", CreatedAtUtc = now },
            new Product { Id = 15, Name = "Bounty Fresh Whole Chicken", Category = "Meat", Price = 180.00m, Quantity = 25, ImageUrl = "/uploads/products/bounty-fresh-chicken.jpg", CreatedAtUtc = now },
            new Product { Id = 16, Name = "Magnolia Gold Butter 200g", Category = "Dairy", Price = 125.00m, Quantity = 40, ImageUrl = "/uploads/products/magnolia-butter.jpg", CreatedAtUtc = now },
            new Product { Id = 17, Name = "Pringles Original 149g", Category = "Snacks", Price = 105.00m, Quantity = 85, ImageUrl = "/uploads/products/pringles-original.jpg", CreatedAtUtc = now },
            new Product { Id = 18, Name = "Colgate Double Action Toothbrush", Category = "Personal Care", Price = 65.00m, Quantity = 110, ImageUrl = "/uploads/products/colgate-toothbrush.jpg", CreatedAtUtc = now },
            new Product { Id = 19, Name = "Safeguard Pure White Soap 130g", Category = "Personal Care", Price = 45.00m, Quantity = 250, ImageUrl = "/uploads/products/safeguard-soap.jpg", CreatedAtUtc = now },
            new Product { Id = 20, Name = "Fita Crackers 250g", Category = "Snacks", Price = 55.00m, Quantity = 90, ImageUrl = "/uploads/products/fita-crackers.jpg", CreatedAtUtc = now }
        );
    }
}
