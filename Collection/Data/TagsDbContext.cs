using Collection.Models;
using Microsoft.EntityFrameworkCore;

namespace Collection.Data
{
    public class TagsDbContext : DbContext
    {
        public TagsDbContext(DbContextOptions<TagsDbContext> options) : base(options)
        {

        }

        public DbSet<Tag> Tags { get; set; }
    }
}
