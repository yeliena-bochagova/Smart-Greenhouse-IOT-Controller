Скопіюйте appsettings.example.json → appsettings.json
Вставте свої ClientId та ClientSecret із Google Cloud Console.

Створення OAuth2-клієнта у Google Cloud Console
1. Перейдіть за посиланням:

    https://console.cloud.google.com/apis/credentials

2. Створіть проект (якщо його ще немає):

    Select project → New Project → створити будь-яку назву.

3. Увімкніть Google Identity / OAuth 2.0 API:

    APIs & Services → Library → Google Identity Services → Enable

4. Створіть OAuth 2.0 Client ID:

    Відкрийте APIs & Services → Credentials

    Натисніть Create Credentials

    Оберіть OAuth Client ID

    Тип застосунку → Web application

    Дайте назву (наприклад, SmartGreenhouse Local)

5. Зареєструйте Redirect URIs

    Додайте такі значення:

    http://localhost:5273/signin-google
    http://localhost:5273/Auth/GoogleResponse


⚠ Якщо ваш порт інший — змініть відповідно.

6. Отримайте ключі

Після створення Google видасть:

**Client ID**

**Client Secret**

✅ 2. Додавання ключів у проект

Файл appsettings.json *не викладається* у GitHub (він у .gitignore).
Тому кожен учасник повинен створити його у себе локально.

1. Скопіюйте файл-шаблон:
appsettings.example.json → appsettings.json

2. Вставте свої ключі:
"Authentication": {
  "Google": {
    "ClientId": "ВАШ_CLIENT_ID",
    "ClientSecret": "ВАШ_CLIENT_SECRET",
    "AuthorizationEndpoint": "https://accounts.google.com/o/oauth2/v2/auth",
    "TokenEndpoint": "https://oauth2.googleapis.com/token",
    "UserInfoEndpoint": "https://openidconnect.googleapis.com/v1/userinfo",
    "Scopes": "openid profile email"
  }
}

✅ 3. Запуск проєкту

У терміналі:

dotnet run


Потім відкрийте:

http://localhost:5273/Auth/Login


При натисканні “Sign in with Google” має відбутися коректний вхі