using Microsoft.EntityFrameworkCore;
using LinkShortener.Models;

namespace LinkShortener.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // This DbSet MUST be named UrlMappings (to match your usage in Program.cs)
        public DbSet<ShortUrl> UrlMappings { get; set; }
        public DbSet<Click> Clicks { get; set; }
    }
}
