using EventCalendar.Auth.Application;
using EventCalendar.Auth.Infrastructure;
using EventCalendar.Auth.Infrastructure.DataAccess;
using EventCalendar.Auth.Middlewares;
using EventCalendar.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

var tokenSettings = builder.Configuration.GetSection("TokenSettings").Get<TokenSettings>()
                    ?? throw new InvalidOperationException("TokenSettings не найдены в конфигурации.");
tokenSettings.Validate();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddSingleton<IOptions<TokenSettings>>(Options.Create(tokenSettings));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddObservability(builder.Configuration);

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console(new CompactJsonFormatter()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.MapPrometheusScrapingEndpoint();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
