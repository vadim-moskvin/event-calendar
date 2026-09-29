# Event Calendar

Учебный проект календаря событий, разделённый на три API-сервиса. Каждый сервис имеет собственные проекты Application, Domain и Infrastructure и отдельную базу PostgreSQL. Общий формат сообщения Kafka находится в `EventCalendar.Contracts`.

## Возможности

- Регистрация пользователей и выдача JWT-токенов.
- Создание, просмотр, изменение и удаление событий с проверкой входных данных.
- Создание, просмотр и отмена броней; фоновое подтверждение и передача сведений о нём через Kafka.
- Swagger UI для ручной проверки API.
- Кэширование отдельных событий и топ-10 событий в Redis.

## Состав системы

| Сервис | Назначение | Локальный API | База PostgreSQL | Порт БД на хосте |
| --- | --- | --- | --- | --- |
| `EventCalendar.Auth` | Регистрация, вход и выдача JWT | `http://localhost:5101` | `users` | `5433` |
| `EventCalendar.Events` | CRUD событий и учёт доступных мест | `http://localhost:5102` | `events` | `5434` |
| `EventCalendar.Bookings` | Создание, просмотр и отмена броней | `http://localhost:5103` | `bookings` | `5435` |

Все сервисы можно запустить через `src/docker-compose.yml`; API также можно запускать отдельно, например из Rider. Kafka доступна приложениям на хосте по адресу `localhost:9092`; для приложений внутри сети Docker используется `kafka:29092`. Строки подключения к БД и адрес брокера заданы в `appsettings.json` соответствующих сервисов и переопределяются переменными среды в Compose. Миграции EF Core применяются при старте API.

## Структура решения

Для каждого сервиса есть четыре проекта с одинаковым разделением ответственности:

| Проект | Ответственность |
| --- | --- |
| `EventCalendar.<Service>.Domain` | Доменные модели, перечисления и исключения. |
| `EventCalendar.<Service>.Application` | Бизнес-правила, сервисы и интерфейсы для работы с внешним миром. |
| `EventCalendar.<Service>.Infrastructure` | Реализации репозиториев, EF Core, внешние клиенты и фоновые компоненты. |
| `EventCalendar.<Service>` | Контроллеры, HTTP-пайплайн, конфигурация и регистрация зависимостей. |

Здесь `<Service>` — `Auth`, `Events` или `Bookings`. `EventCalendar.Contracts` содержит общий контракт `BookingConfirmed` и имя Kafka-топика. Для каждого сервиса также есть проекты `EventCalendar.<Service>.Tests` и `EventCalendar.<Service>.IntegrationTests`.

Чтобы создать новую миграцию для конкретного сервиса из корня репозитория, укажите его Infrastructure и хост-проект. Например, для Events:

```powershell
dotnet ef migrations add <имя миграции> --project src/EventCalendar.Events.Infrastructure --startup-project src/EventCalendar.Events
```

При обычном запуске вручную применять миграции не нужно: хост делает это сам.

## API

| Сервис | Метод и путь | Доступ | Назначение |
| --- | --- | --- | --- |
| Auth | `POST /register` | Открытый | Зарегистрировать пользователя. |
| Auth | `POST /login` | Открытый | Получить JWT по логину и паролю. |
| Events | `GET /events` | Открытый | Получить список событий с фильтрами и пагинацией. |
| Events | `GET /events/{id}` | Открытый | Получить событие по идентификатору. |
| Events | `GET /top10` | Открытый | Получить топ-10 событий. |
| Events | `POST /events` | `Admin` | Создать событие. |
| Events | `PUT /events/{id}` | `Admin` | Полностью обновить событие. |
| Events | `DELETE /events/{id}` | `Admin` | Удалить событие. |
| Bookings | `POST /bookings` | JWT | Создать бронь в статусе `Pending`; ответ `202 Accepted`. |
| Bookings | `GET /bookings/{id}` | JWT | Получить свою бронь; `Admin` может получить любую. |
| Bookings | `DELETE /bookings/{id}` | JWT | Отменить свою бронь; `Admin` может отменить любую. |

`GET /events` принимает необязательные параметры `title` (часть названия), `from` (нижняя граница начала события), `to` (верхняя граница окончания), `page` (номер страницы) и `pageSize` (размер страницы). По умолчанию возвращается первая страница из 10 событий.

Для создания и обновления события передаются `Title`, `Description`, `StartAt`, `EndAt` и `TotalSeats`. Название обязательно, дата начала должна быть раньше даты окончания, а число мест — положительным. В ответе Events также возвращает `Id` и `AvailableSeats`. Новое событие получает `AvailableSeats`, равное `TotalSeats`.

Для `POST /bookings` нужен `EventId`. Bookings сохраняет `UserId` из claim `sub` токена. Ответ о брони содержит `Id`, `EventId`, `Status`, `CreatedAt` и `ProcessedAt`. Статусы модели: `Pending`, `Confirmed`, `Rejected`, `Cancelled`; для ожидающей брони `ProcessedAt` отсутствует. На одного пользователя допускается не более 10 активных броней. При создании Bookings пока не проверяет существование события и число мест в Events.

## Бронирование и Kafka

1. `POST /bookings` создаёт бронь в статусе `Pending`. Bookings сохраняет `UserId` из claim `sub` токена.
2. Фоновый обработчик Bookings переводит бронь в `Confirmed`, сохраняет статус в своей БД и публикует JSON-сообщение `BookingConfirmed` в топик `booking-confirmed`. Ключ Kafka-сообщения — `EventId`.
3. Фоновый подписчик Events с группой `event-calendar-events` получает сообщение и уменьшает `AvailableSeats` на `SeatCount`. При запуске Events сначала пытается создать топик, если его ещё нет.
4. Если событие не найдено, сообщение некорректно или свободных мест недостаточно, Events записывает причину в лог и пропускает сообщение. Подтверждённая бронь при этом остаётся в Bookings: обратного сообщения и согласования статусов пока нет.

Контракт и имя топика определены в `src/EventCalendar.Contracts`. Прямых вызовов между Bookings и Events нет. Повторная доставка Kafka-сообщения пока может повторно уменьшить число мест: учёт уже обработанных `BookingId` ещё не реализован.

## Кэширование событий

Сервис Events использует Redis для чтения отдельного события по `id` и списка топ-10. При запросе он сначала ищет данные в кэше. Если ключа нет, читает данные из PostgreSQL, сохраняет их в Redis с ограниченным сроком жизни и возвращает клиенту. Остальные запросы списка событий не кэшируются.

| Данные | Ключ Redis | TTL | Причина |
| --- | --- | --- | --- |
| Отдельное событие | `event:{id}` | 1 минута | В событии есть число свободных мест. Короткий срок ограничивает время, в течение которого можно получить устаревшие данные при пропущенной инвалидации. |
| Топ-10 событий | `event:top10` | 10 минут | Для этого списка допустимо более редкое обновление. |

Для отдельного события выбрана стратегия **Delete-on-Write**: после изменения или удаления записи в PostgreSQL сервис удаляет её ключ из Redis. Следующее чтение снова загрузит событие из БД. Это позволяет кэшировать только события, которые действительно запрашивали, без обновления всех записей после каждой операции. Предположение, что часть событий не будет востребована, пока не проверено замерами.

## JWT и права доступа

Токен выдаёт только Auth. Он подписывается секретом из переменной среды **`TokenSettings__SecretKey`**. Задайте **одно и то же значение** для Auth, Events и Bookings; ключ должен содержать не менее 32 байт UTF-8. Не добавляйте его в `appsettings.json` или репозиторий. Двойное подчёркивание в имени переменной соответствует ключу конфигурации `TokenSettings:SecretKey`.

Издатель токена — `EventCalendar.Auth`; токен содержит аудитории `EventCalendar.Events` и `EventCalendar.Bookings`. Каждый API проверяет свою аудиторию, издателя, подпись и срок действия токена. `POST`, `PUT` и `DELETE /events` доступны только роли `Admin`; чтение событий открыто. Все эндпоинты Bookings требуют токен. Пользователь может читать и отменять свои брони, администратор — также чужие.

## Запуск через Docker Compose

Из корня репозитория задайте общий JWT-ключ длиной не менее 32 байт и запустите контейнеры:

```powershell
$env:TokenSettings__SecretKey = '<один и тот же ключ длиной не менее 32 байт>'
docker compose -f src/docker-compose.yml up --build -d
docker compose -f src/docker-compose.yml ps
```

API будут доступны на портах 5101 (Auth), 5102 (Events) и 5103 (Bookings), Jaeger — на 16686, Prometheus — на 9090. Dockerfile публикуют приложения в конфигурации `Release`, а контейнеры запускаются с `ASPNETCORE_ENVIRONMENT=Production`. Bookings ждёт, пока Events станет `healthy`. При этом Events не загружает события в Redis при старте: кэш заполняется при чтении событий.

## Наблюдаемость

API используют OpenTelemetry: трассировки отправляются по OTLP в Jaeger, а метрики ASP.NET Core и .NET доступны на `/metrics`. Prometheus опрашивает все три API каждые 15 секунд по настройкам из `src/telemetry/prometheus.yml`; Grafana показывает собранные метрики на дашборде из `src/telemetry/grafana/dashboard.json`.

`ServiceVersion` задаётся для каждого API в его `appsettings.json` (сейчас `1.0.0`). При выпуске новой версии обновите значение нужного сервиса или передайте переменную окружения `ServiceVersion`; OpenTelemetry добавит его как атрибут ресурса `service.version`.

| Инструмент | Назначение | Адрес на хосте |
| --- | --- | --- |
| Grafana | Дашборд и графики метрик | `http://localhost:3000` |
| Prometheus | Сбор метрик и проверка целей в **Status → Targets** | `http://localhost:9090` |
| Jaeger | Просмотр трассировок | `http://localhost:16686` |

Для запуска всего стека из корня репозитория задайте общий JWT-ключ и выполните:

```powershell
$env:TokenSettings__SecretKey = '<один и тот же ключ длиной не менее 32 байт>'
docker compose -f src/docker-compose.yml up --build -d
```

В новой Grafana войдите под `admin` / `admin` и при необходимости смените пароль. Если контейнер уже запускался, действует ранее установленный пароль: данные Grafana сохраняются в томе. Источник данных **Event Calendar Prometheus** (`http://prometheus:9090` внутри сети Docker) и дашборд **Event Calendar HTTP metrics** в папке **Event Calendar** создаются автоматически из файлов в `src/telemetry/grafana/provisioning` и `src/telemetry/grafana/dashboard.json`. Ручное добавление источника и импорт дашборда не требуются. Дашборд показывает скорость HTTP-запросов, количество запросов в обработке, задержку p50/p95/p99 и долю ответов 5xx (error rate). Изменения дашборда, сохранённые через UI Grafana, нужно экспортировать обратно в `src/telemetry/grafana/dashboard.json`: при обновлении файла версия из репозитория заменит изменения из UI. Трассировки API можно искать в Jaeger по именам `auth-service`, `events-service` и `bookings-service`. Порт `4317` предназначен для приёма OTLP, это не UI.

Две дополнительные панели показывают частоту сборок GC по поколениям и состояние пула потоков (очередь задач и число потоков) для выбранного сервиса.

## Локальный запуск

Нужны .NET 10 SDK и Docker. Команды ниже выполняются из корня репозитория.

1. Задайте `TokenSettings__SecretKey` и запустите только инфраструктуру:

   ```powershell
   $env:TokenSettings__SecretKey = '<один и тот же ключ длиной не менее 32 байт>'
   docker compose -f src/docker-compose.yml up -d zookeeper kafka redis prometheus jaeger grafana users-db events-db bookings-db
   docker compose -f src/docker-compose.yml ps
   ```

2. Задайте `TokenSettings__SecretKey` во всех трёх процессах. Для работы из Rider удобно задать пользовательскую переменную среды Windows и перезапустить Rider. Если запускаете API из отдельных терминалов PowerShell, установите переменную в **каждом** терминале:

   ```powershell
   $env:TokenSettings__SecretKey = '<один и тот же ключ длиной не менее 32 байт>'
   ```

3. Запустите сервисы в трёх отдельных терминалах, задав каждому свой HTTP-порт:

   | Сервис | Команды PowerShell |
   | --- | --- |
   | Auth | `$env:ASPNETCORE_URLS = 'http://localhost:5101'`<br>`dotnet run --project src/EventCalendar.Auth/EventCalendar.Auth.csproj --no-launch-profile` |
   | Events | `$env:ASPNETCORE_URLS = 'http://localhost:5102'`<br>`dotnet run --project src/EventCalendar.Events/EventCalendar.Events.csproj --no-launch-profile` |
   | Bookings | `$env:ASPNETCORE_URLS = 'http://localhost:5103'`<br>`dotnet run --project src/EventCalendar.Bookings/EventCalendar.Bookings.csproj --no-launch-profile` |

В Rider уже есть конфигурации `.NET Project` для этих трёх хост-проектов. В **Run → Edit Configurations** добавьте каждому свою переменную `ASPNETCORE_URLS` из таблицы. Если ключ не задан на уровне Windows, добавьте одинаковый `TokenSettings__SecretKey` в каждую конфигурацию. Для одновременного запуска можно создать конфигурацию **Multi-Launch** из трёх существующих конфигураций; инфраструктуру Docker запустите заранее.

Swagger UI доступен по адресам `http://localhost:5101/swagger`, `http://localhost:5102/swagger` и `http://localhost:5103/swagger`. В Auth используйте `POST /register` и `POST /login`, затем вставьте выданный JWT в **Authorize** интерфейсов Events и Bookings. В текущем варианте локальный запуск использует HTTP без перенаправления на HTTPS.

## Сборка и тесты

```powershell
dotnet build src/EventCalendar.sln
dotnet test src/EventCalendar.sln
```

Для каждого сервиса есть отдельные unit- и интеграционные тесты. Unit-тесты используют EF Core InMemory, интеграционные тесты запускают PostgreSQL через Testcontainers; для них требуется работающий Docker.
