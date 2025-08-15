using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace UptimeMonitor.Api.Services;

public static class HttpClientFactoryExtensions
{
    public static IServiceCollection AddMonitorsHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient("monitor")
            .AddResilienceHandler("monitor-pipeline", (builder, context) =>
            {
                // Повтор с экспоненциальной задержкой (3 попытки)
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromMilliseconds(200),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                });

                // Таймаут на уровень запроса (дополнительно к HttpClient.Timeout)
                builder.AddTimeout(TimeSpan.FromSeconds(10));
            });

        return services;
    }
}
