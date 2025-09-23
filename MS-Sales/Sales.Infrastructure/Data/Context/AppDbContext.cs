using Microsoft.EntityFrameworkCore;
using Sales.Domain.Models;

namespace Sales.Infrastructure.Data.Context
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

        public DbSet<Order> Orders { get; set; }

    }
}
