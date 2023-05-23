using Microsoft.EntityFrameworkCore;
using Mercadona4.Models;

namespace Mercadona4.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
    }
}
