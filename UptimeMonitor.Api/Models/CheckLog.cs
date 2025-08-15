namespace UptimeMonitor.Api.Models;

public class CheckLog
{
    public int Id { get; set; }
    public int MonitorTargetId { get; set; }

    public DateTimeOffset CheckedAt { get; set; }
    public bool Ok { get; set; }
    public int StatusCode { get; set; }
    public long LatencyMs { get; set; }
    public string? Error { get; set; }
}
