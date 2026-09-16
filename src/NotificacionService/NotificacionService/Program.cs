using Microsoft.EntityFrameworkCore;
using NotificacionService.Data;
using NotificacionService.Interfaces;
using NotificacionService.Repositories;
using NotificacionService.Services;
using NotificacionService.Workers;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog(config =>
    config.ReadFrom.Configuration(builder.Configuration)
          .WriteTo.Console());

builder.Services.AddDbContext<NotificacionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddScoped<ICargaArchivoRepository, CargaArchivoRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificacionService, NotificacionProcessingService>();

builder.Services.AddHostedService<NotificacionWorker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificacionDbContext>();
    for (var i = 0; i < 10; i++)
    {
        try { db.Database.CanConnect(); break; }
        catch
        {
            Console.WriteLine($"esperando sql server ({i + 1}/10)");
            Thread.Sleep(5000);
        }
    }
}

host.Run();
