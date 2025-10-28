using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stock.Domain.Models;

namespace Stock.Infrastructure.Data.MappingTables;

public class StockReservationsMap : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> e)
    {
        e.HasKey(sr => sr.Id);
        e.Property(sr => sr.OrderId).IsRequired();
        e.Property(sr => sr.ProductId).IsRequired();
        e.Property(sr => sr.Quantity).IsRequired();
        e.Property(sr => sr.ReservedAt).IsRequired();
        e.Property(sr => sr.ExpiresAt).IsRequired();
        e.Property(sr => sr.Status).IsRequired().HasConversion<string>();
        e.ToTable("StockReservations");
    }
}