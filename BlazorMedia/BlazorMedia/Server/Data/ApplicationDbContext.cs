using Microsoft.EntityFrameworkCore;

namespace BlazorMedia.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Reviewer>().HasIndex(nameof(Reviewer.ModifiedTicks), nameof(Reviewer.ReviewerId));
        }

        // Reviewers
        public DbSet<Reviewer> Reviewers { get; set; } = default!;
    }
}
