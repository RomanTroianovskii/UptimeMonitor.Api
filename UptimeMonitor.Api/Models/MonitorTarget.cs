using System.ComponentModel.DataAnnotations;

namespace UptimeMonitor.Api.Models;

public enum CheckMethod { Head, Get }

public class MonitorTarget
{
    public int Id { get; set; }

    [Required] public string Name { get; set; } = default!;
    [Required] public string Url { get; set; } = default!;
    public CheckMethod Method { get; set; } = CheckMethod.Head;

    // Период проверки (сек)
    [Range(5, 86400)] public int IntervalSec { get; set; } = 60;

    // Таймаут HTTP (сек)
    [Range(1, 120)] public int TimeoutSec { get; set; } = 10;

    // Окно для «нестабильно» (сколько фейлов подряд)
    [Range(1, 10)] public int FailThreshold { get; set; } = 3;

    // Включён ли мониторинг
    public bool Enabled { get; set; } = true;

    // Для телеги можно задать chatId, иначе возьмём общий из настроек
    public long? TelegramChatId { get; set; }

    // Технические поля
    public DateTimeOffset? LastCheckedAt { get; set; }
    public bool? LastOk { get; set; }
    public int ConsecutiveFails { get; set; }
}
