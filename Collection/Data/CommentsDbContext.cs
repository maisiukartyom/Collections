using Collection.Models;
using Microsoft.EntityFrameworkCore;

namespace Collection.Data
{
    public class CommentsDbContext : DbContext
    {
        public CommentsDbContext(DbContextOptions<CommentsDbContext> options) : base(options)
        {

        }

        public DbSet<Comment> Comments { get; set; }
    }
}
