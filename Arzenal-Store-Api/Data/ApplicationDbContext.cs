using ArzenalStoreApi.Models;
using Microsoft.EntityFrameworkCore;
using OperatingSystem = ArzenalStoreApi.Models.OperatingSystem;

namespace ArzenalStoreApi.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<App> Apps { get; set; }
        public DbSet<Categorie> Categories { get; set; }
        public DbSet<AppLanguage> AppLanguages { get; set; }
        public DbSet<AppOperatingSystem> AppOperatingSystems { get; set; }
        public DbSet<AppTag> AppTags { get; set; }
        public DbSet<OperatingSystem> OperatingSystems { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Tag> Tags { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .LogTo(Console.WriteLine, LogLevel.Information) // Journalisation des requêtes SQL
                    .EnableSensitiveDataLogging(); // Active les données sensibles dans les journaux (ex. paramètres des requêtes)
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            // Mapper les noms des classes aux noms des tables
            modelBuilder.Entity<App>().ToTable("Apps");
            modelBuilder.Entity<AppLanguage>().ToTable("AppLanguages");
            modelBuilder.Entity<AppTag>().ToTable("AppTags");
            modelBuilder.Entity<AppOperatingSystem>().ToTable("AppOperatingSystems");
            modelBuilder.Entity<OperatingSystem>().ToTable("OperatingSystems");
            modelBuilder.Entity<Language>().ToTable("Languages");
            modelBuilder.Entity<Tag>().ToTable("Tags");
            modelBuilder.Entity<Categorie>().ToTable("Categories");



            // Configuration de App
            modelBuilder.Entity<App>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Version).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(255);

                entity.Property(e => e.Description).HasColumnType("text");
                entity.Property(e => e.Icone).HasMaxLength(255);
                entity.Property(e => e.Requirements).HasColumnType("text");

                entity.Property(e => e.IsVisible).HasDefaultValue(true);
                entity.Property(e => e.ReleaseDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.LastUpdated).IsRequired(false);
                entity.Property(e => e.AppSize);
                entity.Property(e => e.ReleaseDate)
                     .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.LastUpdated)
                    .IsRequired(false)
                    .ValueGeneratedOnUpdate()
                    .HasDefaultValueSql("NULL");

                // Relation avec Categorie
                entity.HasOne(e => e.Category)
                    .WithMany()
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuration de AppLanguage
            modelBuilder.Entity<AppLanguage>()
    .HasKey(al => new { al.AppId, al.LanguageId });

            modelBuilder.Entity<AppLanguage>()
                .HasOne(al => al.App)
                .WithMany(a => a.AppLanguages)
                .HasForeignKey(al => al.AppId);

            modelBuilder.Entity<AppLanguage>()
                .HasOne(al => al.Language)
                .WithMany(l => l.AppLanguages)
                .HasForeignKey(al => al.LanguageId);


            // Configuration de AppTag
            modelBuilder.Entity<AppTag>()
    .HasKey(at => new { at.AppId, at.TagId });

            modelBuilder.Entity<AppTag>()
                .HasOne(at => at.App)
                .WithMany(a => a.AppTags)
                .HasForeignKey(at => at.AppId);

            modelBuilder.Entity<AppTag>()
                .HasOne(at => at.Tag)
                .WithMany(t => t.AppTags)
                .HasForeignKey(at => at.TagId);


            // Configuration de AppOperatingSystem
            modelBuilder.Entity<AppOperatingSystem>()
    .HasKey(ao => new { ao.AppId, ao.OSId });

            modelBuilder.Entity<AppOperatingSystem>()
                .HasOne(ao => ao.App)
                .WithMany(a => a.AppOperatingSystems)
                .HasForeignKey(ao => ao.AppId);

            modelBuilder.Entity<AppOperatingSystem>()
                .HasOne(ao => ao.OperatingSystem)
                .WithMany(os => os.AppOperatingSystems)
                .HasForeignKey(ao => ao.OSId);

        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries<App>()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                entry.Entity.LastUpdated = DateTime.UtcNow;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }



    }
}
