using Microsoft.EntityFrameworkCore;

using BlazorMedia.Client;

namespace BlazorMedia.Client.Data
{
    public class ClientSideDbContext : DbContext
    {
        public DbSet<Reviewer> Reviewers { get; set; } = default!;

        public ClientSideDbContext(DbContextOptions<ClientSideDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reviewer>().HasIndex(nameof(Reviewer.ModifiedTicks), nameof(Reviewer.ReviewerId));
            modelBuilder.Entity<Reviewer>().HasIndex(nameof(Reviewer.Platform), nameof(Reviewer.Genre));
            modelBuilder.Entity<Reviewer>().HasIndex(x => x.Reviews);
            modelBuilder.Entity<Reviewer>().HasIndex(x => x.Name);
            modelBuilder.Entity<Reviewer>().Property(x => x.Name).UseCollation("nocase");
        }
    }
}

