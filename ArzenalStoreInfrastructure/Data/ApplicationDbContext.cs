using Arzenal.Store.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using OperatingSystem = Arzenal.Store.Api.Domain.Models.OperatingSystem;

namespace ArzenalStoreInfrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        { }
        public DbSet<App> Apps { get; set; }
        public DbSet<Categorie> Categories { get; set; }
        public DbSet<AppLanguage> AppLanguages { get; set; }
        public DbSet<AppOperatingSystem> AppOperatingSystems { get; set; }
        public DbSet<AppTag> AppTags { get; set; }
        public DbSet<AppArtifact> AppArtifacts { get; set; }
        public DbSet<OperatingSystem> OperatingSystems { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Release> Releases { get; set; }
        public DbSet<PublicationAudit> PublicationAudits { get; set; }


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
            modelBuilder.Entity<Release>().ToTable("Releases");

            ConfigureApp(modelBuilder);
            ConfigureAppLanguage(modelBuilder);
            ConfigureAppTag(modelBuilder);
            ConfigureAppOperatingSystem(modelBuilder);
            ConfigureAppArtifact(modelBuilder);
            ConfigureRelease(modelBuilder);
            modelBuilder.Entity<PublicationAudit>(entity =>
            {
                entity.ToTable("PublicationAudits");
                entity.HasKey(audit => audit.Id);
                entity.Property(audit => audit.ActorId).IsRequired().HasMaxLength(200);
                entity.Property(audit => audit.Action).IsRequired().HasMaxLength(32);
                entity.Property(audit => audit.OccurredAt).HasColumnType("datetime(6)");
                entity.HasIndex(audit => new { audit.AppId, audit.OccurredAt });
                entity.HasIndex(audit => new { audit.ReleaseId, audit.OccurredAt });
            });


        }

        private static void ConfigureApp(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<App>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Slug).HasMaxLength(160);
                entity.HasIndex(e => e.Slug).IsUnique();
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
                entity.Property(artifact => artifact.Architecture).HasMaxLength(16);
                entity.Property(artifact => artifact.PackageType).HasMaxLength(16);
                entity.Property(artifact => artifact.PackageId).HasMaxLength(255);
                entity.Property(artifact => artifact.PackageVersion).HasMaxLength(50);
                entity.Property(artifact => artifact.MinimumOsVersion).HasMaxLength(50);
                entity.Property(artifact => artifact.Sha256).HasMaxLength(64);
                entity.Property(artifact => artifact.PublisherKeyId).HasMaxLength(100);
                entity.Property(artifact => artifact.PublisherSignature).HasMaxLength(2048);
                entity.HasIndex(artifact => new { artifact.ReleaseId, artifact.Platform, artifact.Architecture, artifact.PackageType }).IsUnique();
                // AppId dans les deux clés empêche de relier un fichier à la release d'une autre app.
                entity.HasOne(artifact => artifact.Release)
                    .WithMany(release => release.Artifacts)
                    .HasForeignKey(artifact => new { artifact.ReleaseId, artifact.AppId })
                    .HasPrincipalKey(release => new { release.Id, release.AppId })
                    .OnDelete(DeleteBehavior.Cascade);
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

        private static void ConfigureRelease(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Release>(entity =>
            {
                entity.HasKey(release => release.Id);
                entity.Property(release => release.Version)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(release => release.Status)
                    .IsRequired()
                    .HasMaxLength(20);
                entity.Property(release => release.Channel).IsRequired().HasMaxLength(16);
                entity.Property(release => release.Changelog).HasMaxLength(16000);
                entity.Property(release => release.CreatedAt)
                    .HasColumnType("datetime(6)");
                entity.Property(release => release.PublishedAt)
                    .HasColumnType("datetime(6)");
                entity.HasOne(release => release.App)
                    .WithMany(app => app.Releases)
                    .HasForeignKey(release => release.AppId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(release => new { release.AppId, release.Version })
                .IsUnique();
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
