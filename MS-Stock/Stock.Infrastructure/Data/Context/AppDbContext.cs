using Microsoft.EntityFrameworkCore;
using Stock.Domain.Models;

namespace Stock.Infrastructure.Data.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        public DbSet<Product> Stock { get; set; }
        public DbSet<StockReservation> StockReservations { get; set; }
    }
}