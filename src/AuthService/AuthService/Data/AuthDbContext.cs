using AuthService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(150).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(e => e.NombreCompleto).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Rol).HasMaxLength(50).HasDefaultValue("User");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).HasMaxLength(500).IsRequired();
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Email = "admin@cargamasiva.com",
            PasswordHash = passwordHash,
            NombreCompleto = "Administrador del Sistema",
            Rol = "Admin",
            FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
