using CargaMasivaService.Data;
using CargaMasivaService.Interfaces;
using CargaMasivaService.Repositories;
using CargaMasivaService.Services;
using CargaMasivaService.Workers;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog(config =>
    config.ReadFrom.Configuration(builder.Configuration)
          .WriteTo.Console());

builder.Services.AddDbContext<CargaMasivaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddScoped<ICargaArchivoRepository, CargaArchivoRepository>();
builder.Services.AddScoped<IDataProcesadaRepository, DataProcesadaRepository>();
builder.Services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();
builder.Services.AddScoped<IExcelProcessorService, ExcelProcessorService>();
builder.Services.AddHttpClient<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<ICargaMasivaService, CargaMasivaProcessingService>();

builder.Services.AddSingleton<IMessagePublisher>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<RabbitMqPublisher>>();
    return RabbitMqPublisher.CreateAsync(config, logger).GetAwaiter().GetResult();
});

builder.Services.AddHostedService<CargaMasivaWorker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CargaMasivaDbContext>();

    for (var i = 0; i < 30; i++)
    {
        try
        {
            var exists = db.Database.ExecuteSqlRaw(
                "SELECT OBJECT_ID(N'CargaArchivo', N'U')");
            var result = db.Database.SqlQueryRaw<int?>(
                "SELECT OBJECT_ID(N'CargaArchivo', N'U') AS [Value]").FirstOrDefault();
            if (result != null) break;
        }
        catch { }
        Console.WriteLine($"Waiting for CargaArchivo table... ({i + 1}/30)");
        Thread.Sleep(5000);
    }

    for (var i = 0; i < 10; i++)
    {
        try { db.Database.Migrate(); break; }
        catch (Exception ex)
        {
            Console.WriteLine($"Migration failed, retrying in 5s... ({i + 1}/10): {ex.Message}");
            Thread.Sleep(5000);
        }
    }

    db.Database.ExecuteSqlRaw(@"
        CREATE OR ALTER PROCEDURE sp_ActualizarEstadoCarga
            @Id INT, @Estado NVARCHAR(50), @MensajeError NVARCHAR(MAX) = NULL
        AS BEGIN SET NOCOUNT ON;
            UPDATE CargaArchivo SET Estado = @Estado,
                MensajeError = CASE WHEN @MensajeError IS NOT NULL THEN @MensajeError ELSE MensajeError END,
                FechaFin = CASE WHEN @Estado IN ('Finalizado','Notificado') THEN GETDATE() ELSE FechaFin END
            WHERE Id = @Id;
        END");
    db.Database.ExecuteSqlRaw(@"
        CREATE OR ALTER PROCEDURE sp_InsertarDataProcesada
            @CargaArchivoId INT, @Periodo NVARCHAR(20), @CodigoProducto NVARCHAR(50),
            @NombreProducto NVARCHAR(200), @Precio DECIMAL(18,2),
            @Estado NVARCHAR(50) = 'Procesado', @MensajeError NVARCHAR(500) = NULL
        AS BEGIN SET NOCOUNT ON;
            IF NOT EXISTS (SELECT 1 FROM DataProcesada WHERE Periodo = @Periodo AND CodigoProducto = @CodigoProducto AND Estado = 'Procesado')
            BEGIN
                INSERT INTO DataProcesada (CargaArchivoId,Periodo,CodigoProducto,NombreProducto,Precio,Estado,MensajeError,FechaRegistro)
                VALUES (@CargaArchivoId,@Periodo,@CodigoProducto,@NombreProducto,@Precio,@Estado,@MensajeError,GETDATE());
                SELECT SCOPE_IDENTITY() AS Id;
            END ELSE BEGIN SELECT -1 AS Id; END
        END");
    db.Database.ExecuteSqlRaw(@"
        CREATE OR ALTER PROCEDURE sp_InsertarAuditoriaFallo
            @CargaArchivoId INT, @Fila INT = NULL, @CodigoProducto NVARCHAR(50) = NULL,
            @MotivoRechazo NVARCHAR(100), @DetalleFallo NVARCHAR(MAX) = NULL
        AS BEGIN SET NOCOUNT ON;
            INSERT INTO AuditoriaFallo (CargaArchivoId,Fila,CodigoProducto,MotivoRechazo,DetalleFallo,FechaRegistro)
            VALUES (@CargaArchivoId,@Fila,@CodigoProducto,@MotivoRechazo,@DetalleFallo,GETDATE());
        END");
    db.Database.ExecuteSqlRaw(@"
        CREATE OR ALTER PROCEDURE sp_ObtenerHistorialCargas
            @Usuario NVARCHAR(150) = NULL, @PageNumber INT = 1, @PageSize INT = 20
        AS BEGIN SET NOCOUNT ON;
            SELECT Id,NombreArchivo,Usuario,FechaRegistro,Estado,Periodo,RutaArchivo,FechaFin,MensajeError
            FROM CargaArchivo WHERE (@Usuario IS NULL OR Usuario = @Usuario)
            ORDER BY FechaRegistro DESC OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
        END");
    db.Database.ExecuteSqlRaw(@"
        CREATE OR ALTER PROCEDURE sp_ValidarPeriodo
            @Periodo NVARCHAR(20), @ExisteActiva BIT OUTPUT, @ExisteFinalizada BIT OUTPUT
        AS BEGIN SET NOCOUNT ON;
            SET @ExisteActiva = CASE WHEN EXISTS (
                SELECT 1 FROM CargaArchivo WHERE Periodo=@Periodo AND Estado IN ('Pendiente','EnProceso')
            ) THEN 1 ELSE 0 END;
            SET @ExisteFinalizada = CASE WHEN EXISTS (
                SELECT 1 FROM CargaArchivo WHERE Periodo=@Periodo AND Estado IN ('Cargado','Finalizado','Notificado')
            ) THEN 1 ELSE 0 END;
        END");
}

host.Run();
