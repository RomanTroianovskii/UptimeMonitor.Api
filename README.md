# UptimeMonitor.Api

Минимальный API-сервис для мониторинга HTTP/HTTPS-ресурсов. Приложение хранит список проверяемых URL в SQLite, периодически опрашивает их в фоне, пишет историю проверок и отправляет уведомления в Telegram при смене состояния или серии ошибок.

## Возможности

- CRUD для списка мониторов через HTTP API
- фоновые проверки `HEAD` или `GET`
- хранение логов проверок в SQLite
- автоприменение EF Core миграций при старте
- Swagger UI для ручной работы с API
- Telegram-уведомления о падении, восстановлении и серии ошибок подряд

## Стек

- .NET 8
- ASP.NET Core Minimal API
- Entity Framework Core + SQLite
- Swagger / Swashbuckle
- Telegram.Bot

## Структура проекта

```text
.
+-- README.md
+-- UptimeMonitor.Api/
    +-- UptimeMonitor.Api.sln
    +-- UptimeMonitor.Api/
        +-- Program.cs
        +-- appsettings.json
        +-- Endpoints/
        +-- Models/
        +-- Services/
```

## Требования

- .NET SDK 8.0+
- доступ в интернет для `dotnet restore`
- Telegram bot token и chat id, если нужны уведомления

## Установка

1. Перейдите в корень репозитория:

```powershell
cd "F:\Freelance\Test Project"
```

2. Восстановите зависимости:

```powershell
dotnet restore .\UptimeMonitor.Api\UptimeMonitor.Api.sln
```

3. При необходимости задайте конфигурацию через переменные окружения:

```powershell
$env:ConnectionStrings__db = "Data Source=uptime.db"
$env:Telegram__BotToken = "YOUR_BOT_TOKEN"
$env:Telegram__DefaultChatId = "123456789"
```

4. Запустите приложение:

```powershell
dotnet run --project .\UptimeMonitor.Api\UptimeMonitor.Api\UptimeMonitor.Api.csproj
```

По `launchSettings.json` локальный запуск в development-профиле использует:

- `http://localhost:5054`
- `https://localhost:7151`

При старте приложение автоматически применяет миграции к базе данных.

## Конфигурация

Основные настройки находятся в `UptimeMonitor.Api/UptimeMonitor.Api/appsettings.json`.

Пример:

```json
{
  "ConnectionStrings": {
    "db": "Data Source=uptime.db"
  },
  "Telegram": {
    "BotToken": "YOUR_BOT_TOKEN",
    "DefaultChatId": 123456789
  }
}
```

Рекомендуется не хранить реальные секреты в репозитории, а передавать их через environment variables или user secrets.

## Использование

После запуска откройте Swagger UI:

- `http://localhost:5054/swagger`
- или `https://localhost:7151/swagger`

Дополнительные маршруты:

- `GET /` - редирект на Swagger
- `GET /health` - простая проверка работоспособности

### API мониторов

#### Получить список мониторов

```http
GET /api/monitors
```

#### Создать монитор

```http
POST /api/monitors
Content-Type: application/json
```

```json
{
  "name": "Google",
  "url": "https://www.google.com",
  "method": 0,
  "intervalSec": 60,
  "timeoutSec": 10,
  "failThreshold": 3,
  "enabled": true,
  "telegramChatId": 123456789
}
```

Значения поля `method`:

- `0` - `Head`
- `1` - `Get`

#### Обновить монитор

```http
PUT /api/monitors/{id}
Content-Type: application/json
```

Тело запроса имеет ту же структуру, что и при создании.

#### Удалить монитор

```http
DELETE /api/monitors/{id}
```

#### Получить логи проверок

```http
GET /api/monitors/{id}/logs?take=100
```

Параметр `take` обязателен и ограничивается диапазоном от `1` до `2000`.

## Как работает мониторинг

- фоновый сервис раз в секунду проверяет, какие мониторы пора опросить
- для каждого активного монитора отправляется `HEAD` или `GET`
- результат записывается в таблицу логов
- при смене состояния `up/down` отправляется Telegram-уведомление
- если ошибок подряд стало ровно столько, сколько указано в `FailThreshold`, отправляется дополнительное предупреждение

## База данных

По умолчанию используется SQLite-файл:

```text
UptimeMonitor.Api/UptimeMonitor.Api/uptime.db
```

Структура БД создается и обновляется автоматически через EF Core migrations во время старта приложения.

## Полезные команды

Сборка:

```powershell
dotnet build .\UptimeMonitor.Api\UptimeMonitor.Api.sln
```

Запуск:

```powershell
dotnet run --project .\UptimeMonitor.Api\UptimeMonitor.Api\UptimeMonitor.Api.csproj
```

