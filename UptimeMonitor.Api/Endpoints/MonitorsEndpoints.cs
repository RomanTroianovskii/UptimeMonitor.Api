using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Api.Models;

namespace UptimeMonitor.Api.Endpoints;

public static class MonitorsEndpoints
{
    public static IEndpointRouteBuilder MapMonitors(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/monitors");

        g.MapGet("/", async (AppDb db) =>
            await db.Targets.AsNoTracking().ToListAsync());
        g.MapGet("/{id:int}/logs", async (int id, int take, AppDb db) =>
        {
            // сначала забираем записи из БД без ORDER BY по DateTimeOffset
            var raw = await db.Logs.AsNoTracking()
                .Where(l => l.MonitorTargetId == id)
                .Take(Math.Clamp(take, 1, 2000))
                .ToListAsync();

            // сортируем уже в памяти по времени (DESC)
            return raw.OrderByDescending(l => l.CheckedAt).ToList();
        });


        g.MapPost("/", async (MonitorTarget t, AppDb db) =>
        {
            db.Targets.Add(t);
            await db.SaveChangesAsync();
            return Results.Created($"/api/monitors/{t.Id}", t);
        });

        g.MapPut("/{id:int}", async Task<Results<NotFound, Ok<MonitorTarget>>> (int id, MonitorTarget m, AppDb db) =>
        {
            var e = await db.Targets.FindAsync(id);
            if (e is null) return TypedResults.NotFound();
            db.Entry(e).CurrentValues.SetValues(m);
            await db.SaveChangesAsync();
            return TypedResults.Ok(e);
        });

        g.MapDelete("/{id:int}", async (int id, AppDb db) =>
        {
            var e = await db.Targets.FindAsync(id);
            if (e is null) return Results.NotFound();
            db.Remove(e); await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }
}
