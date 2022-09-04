using Microsoft.EntityFrameworkCore;
using Collection.Models;

namespace Collection.Data
{
    public class CollectionsPropDbContext : DbContext
    {
        public CollectionsPropDbContext(DbContextOptions<CollectionsPropDbContext> options) : base(options)
        {

        }

        public DbSet<CollectionProp> CollectionsProps { get; set; }
    }
}
