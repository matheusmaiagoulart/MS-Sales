using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Models;

namespace Sales.Infrastructure.Data.MappingTables
{
    public class OrderMap : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> e)
        {
            e.HasKey(o => o.IdSale);
            e.OwnsMany(o => o.OrdemItens, p =>
            {
                p.WithOwner().HasForeignKey("OrderIdSale");
                p.Property<Guid>("OrderIdSale");
                p.Property(pi => pi.IdProduct).IsRequired();
                p.Property(pi => pi.Quantity).IsRequired();
                p.ToTable("OrdemItem");
            });
            e.Property(o => o.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");
            e.Property(o => o.Status).IsRequired().HasConversion<string>();
            e.Property(o => o.CreatedAt).IsRequired();
            e.Property(o => o.UpdatedAt).IsRequired();
            e.ToTable("Orders");
        }
    }
}
