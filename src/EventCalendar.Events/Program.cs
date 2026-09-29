using System.Text;
using EventCalendar.Events;
using EventCalendar.Events.Application;
using EventCalendar.Events.Infrastructure;
using EventCalendar.Events.Infrastructure.DataAccess;
using EventCalendar.Events.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("TokenSettings").Get<JwtSettings>()
                  ?? throw new InvalidOperationException("TokenSettings не найдены в конфигурации.");
jwtSettings.Validate();

var cacheSettings = builder.Configuration.GetSection("CacheSettings").Get<CacheSettings>()
                    ?? throw new InvalidOperationException("CacheSettings не найдены в конфигурации.");
cacheSettings.Validate();
builder.Services.AddSingleton(cacheSettings);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var kafkaSettings = new KafkaSettings(
    builder.Configuration["Kafka:BootstrapServers"]
    ?? throw new InvalidOperationException("Kafka:BootstrapServers not found."),
    builder.Configuration["Kafka:ConsumerGroup"]
    ?? throw new InvalidOperationException("Kafka:ConsumerGroup not found."));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            RoleClaimType = "role",
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
        };
    });
builder.Services.AddAuthorization();

var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ??
                            throw new InvalidOperationException("Redis connection string not found.");
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnectionString)
);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString, kafkaSettings);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName: "events-service"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console(new CompactJsonFormatter()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EventsDbContext>();
    db.Database.Migrate();
}

app.MapPrometheusScrapingEndpoint();
app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();