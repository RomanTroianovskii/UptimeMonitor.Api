using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Api.Endpoints;
using UptimeMonitor.Api.Models;
using UptimeMonitor.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDb>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("db") ?? "Data Source=uptime.db"));

builder.Services.AddMonitorsHttpClient();
builder.Services.AddSingleton<ITelegramAlertService, TelegramAlertService>();
builder.Services.AddHostedService<UptimeWorker>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    db.Database.Migrate(); 
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/health", () => Results.Ok(new { ok = true, time = DateTimeOffset.UtcNow }));
app.MapMonitors();

app.Run();
