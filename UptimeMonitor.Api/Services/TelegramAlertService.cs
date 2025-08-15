using Telegram.Bot;
using Telegram.Bot.Types;

namespace UptimeMonitor.Api.Services;

public interface ITelegramAlertService
{
    Task NotifyAsync(long chatId, string text, CancellationToken ct = default);
    Task NotifyDefaultAsync(string text, CancellationToken ct = default);
}

public class TelegramAlertService : ITelegramAlertService
{
    private readonly ITelegramBotClient? _bot;
    private readonly long? _defaultChat;

    public TelegramAlertService(IConfiguration cfg)
    {
        var token = cfg["Telegram:BotToken"];
        _defaultChat = cfg.GetValue<long?>("Telegram:DefaultChatId");
        _bot = string.IsNullOrWhiteSpace(token) ? null : new TelegramBotClient(token);
    }

    public async Task NotifyAsync(long chatId, string text, CancellationToken ct = default)
    {
        if (_bot is null) return;
        await _bot.SendMessage(
            chatId: new ChatId(chatId),
            text: text,
            cancellationToken: ct
        );
    }

    public async Task NotifyDefaultAsync(string text, CancellationToken ct = default)
    {
        if (_bot is null || _defaultChat is null) return;
        await _bot.SendMessage(
            chatId: new ChatId(_defaultChat.Value),
            text: text,
            cancellationToken: ct
        );
    }
}
