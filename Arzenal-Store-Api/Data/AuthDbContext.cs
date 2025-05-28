using ArzenalStoreApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ArzenalStoreApi.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<InviteToken> InviteTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<InviteToken>().ToTable("InviteTokens");
            modelBuilder.Entity<User>().ToTable("User");

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                //entity.Property(e => e.Group).IsRequired().HasMaxLength(20);
            });
        }
    }
}
