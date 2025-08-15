using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Api.Models;

namespace UptimeMonitor.Api.Services;

public class UptimeWorker : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly IHttpClientFactory _http;
    private readonly ITelegramAlertService _tg;
    private readonly ILogger<UptimeWorker> _log;

    public UptimeWorker(IServiceProvider sp, IHttpClientFactory http, ITelegramAlertService tg, ILogger<UptimeWorker> log)
    { _sp = sp; _http = http; _tg = tg; _log = log; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("UptimeWorker started");
        // Простой тикер раз в секунду
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await TickAsync(stoppingToken); }
            catch (Exception ex) { _log.LogError(ex, "Tick error"); }
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDb>();
        var now = DateTimeOffset.UtcNow;

        var targets = await db.Targets.Where(t => t.Enabled).ToListAsync(ct);
        foreach (var t in targets)
        {
            var due = t.LastCheckedAt is null || (now - t.LastCheckedAt.Value).TotalSeconds >= t.IntervalSec;
            if (!due) continue;

            var sw = Stopwatch.StartNew();
            var client = _http.CreateClient("monitor");
            client.Timeout = TimeSpan.FromSeconds(Math.Max(1, t.TimeoutSec));

            bool ok = false; int code = 0; string? err = null;
            try
            {
                using var req = new HttpRequestMessage(
                    t.Method == CheckMethod.Head ? HttpMethod.Head : HttpMethod.Get, t.Url);
                var res = await client.SendAsync(req, ct);
                code = (int)res.StatusCode;
                ok = res.IsSuccessStatusCode;
            }
            catch (Exception ex) { err = ex.Message; ok = false; }

            sw.Stop();

            db.Logs.Add(new CheckLog
            {
                MonitorTargetId = t.Id,
                CheckedAt = now,
                Ok = ok,
                StatusCode = code,
                LatencyMs = sw.ElapsedMilliseconds,
                Error = err
            });

            // состояние стабильности
            if (ok) t.ConsecutiveFails = 0;
            else t.ConsecutiveFails++;

            var prevOk = t.LastOk;
            t.LastOk = ok;
            t.LastCheckedAt = now;

            await db.SaveChangesAsync(ct);

            // Алёрты при изменении состояния / превышении порога
            if (prevOk != ok)
            {
                var chat = t.TelegramChatId;
                var msg = ok
                    ? $"✅ {t.Name} поднялся: {t.Url}"
                    : $"❌ {t.Name} недоступен: {t.Url} (код {code}, {err})";
                if (chat.HasValue) await _tg.NotifyAsync(chat.Value, msg, ct);
                else await _tg.NotifyDefaultAsync(msg, ct);
            }
            else if (!ok && t.ConsecutiveFails == t.FailThreshold)
            {
                var msg = $"⚠️ {t.Name}: {t.FailThreshold} ошибок подряд ({t.Url})";
                if (t.TelegramChatId.HasValue) await _tg.NotifyAsync(t.TelegramChatId.Value, msg, ct);
                else await _tg.NotifyDefaultAsync(msg, ct);
            }
        }
    }
}
