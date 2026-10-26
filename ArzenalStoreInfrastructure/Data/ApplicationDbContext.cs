using Arzenal.Store.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using OperatingSystem = Arzenal.Store.Api.Domain.Models.OperatingSystem;

namespace ArzenalStoreInfrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {}
            public DbSet<App> Apps { get; set; }
            public DbSet<Categorie> Categories { get; set; }
            public DbSet<AppLanguage> AppLanguages { get; set; }
            public DbSet<AppOperatingSystem> AppOperatingSystems { get; set; }
            public DbSet<AppTag> AppTags { get; set; }
            public DbSet<AppArtifact> AppArtifacts { get; set; }
            public DbSet<OperatingSystem> OperatingSystems { get; set; }
            public DbSet<Language> Languages { get; set; }
            public DbSet<Tag> Tags { get; set; }


            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {


                // Mapper les noms des classes aux noms des tables
                modelBuilder.Entity<App>().ToTable("Apps");
                modelBuilder.Entity<AppLanguage>().ToTable("AppLanguages");
                modelBuilder.Entity<AppTag>().ToTable("AppTags");
                modelBuilder.Entity<AppArtifact>().ToTable("AppArtifacts");
                modelBuilder.Entity<AppOperatingSystem>().ToTable("AppOperatingSystems");
                modelBuilder.Entity<OperatingSystem>().ToTable("OperatingSystems");
                modelBuilder.Entity<Language>().ToTable("Languages");
                modelBuilder.Entity<Tag>().ToTable("Tags");
                modelBuilder.Entity<Categorie>().ToTable("Categories");

                ConfigureApp(modelBuilder);
                ConfigureAppLanguage(modelBuilder);
                ConfigureAppTag(modelBuilder);
                ConfigureAppOperatingSystem(modelBuilder);
                ConfigureAppArtifact(modelBuilder);

            }

            private static void ConfigureApp(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<App>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                    entity.Property(e => e.Version).IsRequired().HasMaxLength(50);
                    entity.Property(e => e.Description).HasColumnType("text");
                    entity.Property(e => e.Icone).HasMaxLength(255);
                    entity.Property(e => e.Requirements).HasColumnType("text");
                    entity.Property(e => e.IsVisible).HasDefaultValue(true);
                    entity.Property(e => e.ReleaseDate).HasColumnType("datetime(6)").HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
                    entity.Property(e => e.LastUpdated).HasColumnType("datetime(6)").ValueGeneratedOnUpdate()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)").IsRequired(false);
                    entity.Property(e => e.AppSize);

                    // Relation avec Categorie
                    entity.HasOne(e => e.Category)
                        .WithMany()
                        .HasForeignKey(e => e.CategoryId)
                        .IsRequired(false)
                        .OnDelete(DeleteBehavior.SetNull);
                });
            }

            private static void ConfigureAppLanguage(ModelBuilder modelBuilder)
            {
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
            }

            private static void ConfigureAppTag(ModelBuilder modelBuilder)
            {
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
            }

            private static void ConfigureAppOperatingSystem(ModelBuilder modelBuilder)
            {
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

            private static void ConfigureAppArtifact(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<AppArtifact>(entity =>
                {
                    entity.HasKey(artifact => artifact.Id);
                    entity.Property(artifact => artifact.Type).IsRequired().HasMaxLength(16);
                    entity.Property(artifact => artifact.Locator).IsRequired().HasMaxLength(160);
                    entity.Property(artifact => artifact.Version).HasMaxLength(64);
                    entity.Property(artifact => artifact.Platform).HasMaxLength(16);
                    entity.Property(artifact => artifact.OriginalFileName).IsRequired().HasMaxLength(255);
                    entity.Property(artifact => artifact.RelativePath).IsRequired().HasMaxLength(512);
                    entity.Property(artifact => artifact.ContentType).IsRequired().HasMaxLength(128);
                    entity.Property(artifact => artifact.UploadedAt).HasColumnType("datetime(6)");
                    entity.HasIndex(artifact => new { artifact.AppId, artifact.Type, artifact.Locator }).IsUnique();
                    entity.HasOne(artifact => artifact.App)
                        .WithMany(app => app.Artifacts)
                        .HasForeignKey(artifact => artifact.AppId)
                        .OnDelete(DeleteBehavior.Cascade);
                });
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
