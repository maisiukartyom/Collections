using Collection.Models;
using Microsoft.EntityFrameworkCore;

namespace Collection.Data
{
    public class ItemsDbContext : DbContext
    {
        public ItemsDbContext(DbContextOptions<ItemsDbContext> options) : base(options)
        {

        }

        public DbSet<Item> Items { get; set; }
    }
}
