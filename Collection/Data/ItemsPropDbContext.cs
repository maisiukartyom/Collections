using Microsoft.EntityFrameworkCore;
using Collection.Models;

namespace Collection.Data
{
    public class ItemsPropDbContext : DbContext
    {
        public ItemsPropDbContext(DbContextOptions<ItemsPropDbContext> options) : base(options)
        {

        }

        public DbSet<ItemProp> ItemsProps { get; set; }
    }
}
