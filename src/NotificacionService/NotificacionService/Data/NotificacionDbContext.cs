using Microsoft.EntityFrameworkCore;
using NotificacionService.Models.Entities;

namespace NotificacionService.Data;

public class NotificacionDbContext : DbContext
{
    public NotificacionDbContext(DbContextOptions<NotificacionDbContext> options) : base(options) { }

    public DbSet<CargaArchivo> CargaArchivos => Set<CargaArchivo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CargaArchivo>(entity =>
        {
            entity.ToTable("CargaArchivo", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NombreArchivo).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Usuario).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Estado).HasMaxLength(50).HasDefaultValue("Pendiente");
            entity.Property(e => e.Periodo).HasMaxLength(200);
            entity.Property(e => e.RutaArchivo).HasMaxLength(500);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");
        });
    }
}
