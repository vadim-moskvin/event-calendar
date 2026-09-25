# Event Calendar

Учебный проект календаря событий, разделённый на три API-сервиса. Каждый сервис имеет собственные проекты Application, Domain и Infrastructure и отдельную базу PostgreSQL. Общий формат сообщения Kafka находится в `EventCalendar.Contracts`.

## Возможности

- Регистрация пользователей и выдача JWT-токенов.
- Создание, просмотр, изменение и удаление событий с проверкой входных данных.
- Создание, просмотр и отмена броней; фоновое подтверждение и передача сведений о нём через Kafka.
- Swagger UI для ручной проверки API.

## Состав системы

| Сервис | Назначение | Локальный API | База PostgreSQL | Порт БД на хосте |
| --- | --- | --- | --- | --- |
| `EventCalendar.Auth` | Регистрация, вход и выдача JWT | `http://localhost:5101` | `users` | `5433` |
| `EventCalendar.Events` | CRUD событий и учёт доступных мест | `http://localhost:5102` | `events` | `5434` |
| `EventCalendar.Bookings` | Создание, просмотр и отмена броней | `http://localhost:5103` | `bookings` | `5435` |

Базы и Kafka с ZooKeeper запускаются через `src/docker-compose.yml`. API-сервисы запускаются отдельно, например из Rider. Kafka доступна приложениям на хосте по адресу `localhost:9092`; для приложений внутри сети Docker используется `kafka:29092`. Строки подключения к БД и адрес брокера заданы в `appsettings.json` соответствующих сервисов и могут быть переопределены переменными среды. Миграции EF Core применяются при старте API.

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

## JWT и права доступа

Токен выдаёт только Auth. Он подписывается секретом из переменной среды **`TokenSettings__SecretKey`**. Задайте **одно и то же значение** для Auth, Events и Bookings; ключ должен содержать не менее 32 байт UTF-8. Не добавляйте его в `appsettings.json` или репозиторий. Двойное подчёркивание в имени переменной соответствует ключу конфигурации `TokenSettings:SecretKey`.

Издатель токена — `EventCalendar.Auth`; токен содержит аудитории `EventCalendar.Events` и `EventCalendar.Bookings`. Каждый API проверяет свою аудиторию, издателя, подпись и срок действия токена. `POST`, `PUT` и `DELETE /events` доступны только роли `Admin`; чтение событий открыто. Все эндпоинты Bookings требуют токен. Пользователь может читать и отменять свои брони, администратор — также чужие.

## Локальный запуск

Нужны .NET 10 SDK и Docker. Команды ниже выполняются из корня репозитория.

1. Запустите инфраструктуру и дождитесь статуса `healthy`:

   ```powershell
   docker compose -f src/docker-compose.yml up -d
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
