using ControlService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControlService.Data;

public class ControlDbContext : DbContext
{
    public ControlDbContext(DbContextOptions<ControlDbContext> options) : base(options) { }

    public DbSet<CargaArchivo> CargaArchivos => Set<CargaArchivo>();
    public DbSet<DataProcesada> DataProcesada => Set<DataProcesada>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CargaArchivo>(entity =>
        {
            entity.ToTable("CargaArchivo");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NombreArchivo).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Usuario).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Estado).HasMaxLength(50).HasDefaultValue("Pendiente");
            entity.Property(e => e.Periodo).HasMaxLength(200);
            entity.Property(e => e.RutaArchivo).HasMaxLength(500);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<DataProcesada>(entity =>
        {
            entity.ToTable("DataProcesada", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Periodo).HasMaxLength(20).IsRequired();
            entity.Property(e => e.CodigoProducto).HasMaxLength(50).IsRequired();
            entity.Property(e => e.NombreProducto).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Precio).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Estado).HasMaxLength(50).HasDefaultValue("Procesado");
            entity.Property(e => e.MensajeError).HasMaxLength(500);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");
        });
    }
}
