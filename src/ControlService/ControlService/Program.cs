using System.Text;
using ControlService.Data;
using ControlService.Interfaces;
using ControlService.Middleware;
using ControlService.Repositories;
using ControlService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .WriteTo.Console());

builder.Services.AddDbContext<ControlDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!))
        };
    });

builder.Services.AddScoped<ICargaArchivoRepository, CargaArchivoRepository>();
builder.Services.AddHttpClient<IFileStorageService, FileStorageService>();

builder.Services.AddSingleton<IMessagePublisher>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<RabbitMqPublisher>>();
    return RabbitMqPublisher.CreateAsync(config, logger).GetAwaiter().GetResult();
});

builder.Services.AddScoped<ICargaService, CargaService>();
builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ControlDbContext>();
    for (var i = 0; i < 20; i++)
    {
        try { db.Database.Migrate(); break; }
        catch (Exception ex)
        {
            Log.Warning(ex, "Migration failed, retrying in 5s... ({Attempt}/20)", i + 1);
            Thread.Sleep(5000);
        }
    }
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
