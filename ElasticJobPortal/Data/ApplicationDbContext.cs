using ElasticJobPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace ElasticJobPortal.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Job> Jobs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure entity properties and relationships here
            // Example:
            // modelBuilder.Entity<Job>().ToTable("Jobs");
        }
    }
}