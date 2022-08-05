using Microsoft.EntityFrameworkCore;
using Collection.Models;

namespace Collection.Data
{
    public class CollectionsDbContext : DbContext
    {
        public CollectionsDbContext(DbContextOptions<CollectionsDbContext> options) : base(options)
        {

        }

        public DbSet<MCollection> Collections { get; set; }
    }
}
