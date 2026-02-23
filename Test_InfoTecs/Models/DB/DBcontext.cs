using Microsoft.EntityFrameworkCore;

namespace Test_InfoTecs.Models.DB
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ValueRecord> Values { get; set; }
        public DbSet<ResultRecord> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
