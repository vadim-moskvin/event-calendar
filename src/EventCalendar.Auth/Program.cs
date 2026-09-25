using EventCalendar.Auth.Application;
using EventCalendar.Auth.Infrastructure;
using EventCalendar.Auth.Infrastructure.DataAccess;
using EventCalendar.Auth.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.MapControllers();

app.Run();
