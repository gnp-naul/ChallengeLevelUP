using Api.Domain.Entitys;
using Microsoft.EntityFrameworkCore;

namespace Api.Infra.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<GameRecommendation> GameRecommendations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GameRecommendation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Genre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Link).HasMaxLength(100);
                entity.Property(e => e.Platform).HasMaxLength(50);
                entity.Property(e => e.RecommendedAt).IsRequired();
            });
        }
    }
}
