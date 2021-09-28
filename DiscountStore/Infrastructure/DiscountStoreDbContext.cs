using DiscountStore.Modules.Discounts.Models;
using DiscountStore.Modules.Products.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscountStore.Infrastructure
{
    public class DiscountStoreDbContext : DbContext
    {
        public DiscountStoreDbContext(DbContextOptions<DiscountStoreDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<Discount> Discounts { get; set; }
    }
}