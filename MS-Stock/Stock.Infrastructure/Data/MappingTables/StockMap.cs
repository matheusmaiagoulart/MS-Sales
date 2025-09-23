using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stock.Domain.Models;

namespace Stock.Infrastructure.Data.MappingTables
{
    public class StockMap : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> e)
        {
            e.HasKey(p => p.IdProduct);
            e.Property(p => p.Name).IsRequired().HasMaxLength(100);
            e.Property(p => p.Description).IsRequired().HasMaxLength(500);
            e.Property(p => p.Price).IsRequired().HasColumnType("decimal(18,2)");
            e.Property(p => p.StockQuantity).IsRequired();
            e.ToTable("Products");
        }
    }
}