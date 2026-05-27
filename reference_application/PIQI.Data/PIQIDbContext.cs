using Microsoft.EntityFrameworkCore;

namespace PIQI.Data
{
    public class PIQIDbContext : DbContext
    {
        public PIQIDbContext(DbContextOptions<PIQIDbContext> options)
            : base(options)
        {
        }

        public DbSet<PrimaryUnitMart> PrimaryUnitMart { get; set; }
        public DbSet<RangeSetMart> RangeSetMart { get; set; }
        public DbSet<TextMart> TextMart { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            SeedData.Seed(modelBuilder);

            modelBuilder.Entity<PrimaryUnitMart>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.ContentSetMnemonic).HasMaxLength(255).IsRequired();
                entity.Property(e => e.CodeSystemMnemonic).HasMaxLength(100).IsRequired();
                entity.Property(e => e.CodeValue).HasMaxLength(50).IsRequired();
                entity.Property(e => e.UOMText).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => new { e.ContentSetMnemonic, e.CodeSystemMnemonic, e.CodeValue });
            });


            modelBuilder.Entity<RangeSetMart>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.ContentSetMnemonic).HasMaxLength(255).IsRequired();
                entity.Property(e => e.CodeSystemMnemonic).HasMaxLength(100).IsRequired();
                entity.Property(e => e.CodeValue).HasMaxLength(50).IsRequired();
                entity.Property(e => e.UOMText).HasMaxLength(50).IsRequired();
                entity.Property(e => e.MinValue);
                entity.Property(e => e.MaxValue);
                entity.HasIndex(e => new { e.ContentSetMnemonic, e.CodeSystemMnemonic, e.CodeValue });
            });

            modelBuilder.Entity<TextMart>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.ContentSetMnemonic).HasMaxLength(255).IsRequired();
                entity.Property(e => e.TextValue).HasMaxLength(255).IsRequired();
                entity.HasIndex(e => new { e.ContentSetMnemonic }); //TODO: Maybe add TextValue to index as well depending on query patterns
            });
        }
    }

    public class PrimaryUnitMart
    {
        public int Id { get; set; } 
        public string ContentSetMnemonic { get; set; } 
        public string CodeSystemMnemonic { get; set; }
        public string CodeValue { get; set; }
        public string UOMText { get; set; }
    }

    public class RangeSetMart
    {
        public int Id { get; set; } 
        public string ContentSetMnemonic { get; set; }
        public string CodeSystemMnemonic { get; set; }
        public string CodeValue { get; set; }
        public string UOMText { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
    }

    public class TextMart
    {
        public int Id { get; set; } 
        public string ContentSetMnemonic { get; set; } 
        public string TextValue { get; set; }
    }
}