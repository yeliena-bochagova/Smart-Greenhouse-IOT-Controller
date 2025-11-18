# SmartGreenhouse.Web — Лабораторна 3 (ASP.NET Core MVC + OAuth2)

## Структура
- Controllers/ — контролери (HomeController, AuthController, Subroutine1/2/3)
- Models/ — моделі (UserRegistrationModel, UserLoginModel, SubroutineXModel)
- Views/ — Razor views
- Services/ — UserStore (in-memory)
- wwwroot/ — статичні файли, скриншоти
- appsettings.json — налаштування (OAuth ClientId/Secret тут, якщо використовуєш)

## Запуск
1. `dotnet run` у папці Smart-Greenhouse.Web
2. Перейти на http://localhost:5273

## Валідації
- Username: унікальний, max 50 chars
- FullName: max 500 chars
- Password: 8–16 chars, ≥1 digit, ≥1 special symbol, ≥1 uppercase letter
- Phone: +380XXXXXXXXX
- Email: стандартний `EmailAddress`

## Авторизація
- Cookie-based authentication налаштовано.
- Приклад Google OAuth налаштований (потрібні ClientId/Secret в `appsettings.json`).

## Скриншоти
Дивись `wwwroot/screenshots/`
