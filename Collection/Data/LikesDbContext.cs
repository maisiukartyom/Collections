using Collection.Models;
using Microsoft.EntityFrameworkCore;

namespace Collection.Data
{
    public class LikesDbContext : DbContext
    {
        public LikesDbContext(DbContextOptions<LikesDbContext> options) : base(options)
        {

        }

        public DbSet<Like> Likes { get; set; }
    }
}
