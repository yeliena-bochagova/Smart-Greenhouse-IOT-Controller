using SmartGreenhouse.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace SmartGreenhouse.Web.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            // 1. Розумне застосування міграцій
            // Якщо це In-Memory база (для тестів), міграції не потрібні, просто створюємо структуру.
            // Якщо це справжня SQL база - накочуємо міграції.
            if (context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                context.Database.EnsureCreated();
            }
            else
            {
                // Для SQLite, SQL Server, Postgres
                context.Database.Migrate();
            }

            // 2. Якщо база вже має дані (рослини), нічого не робимо
            if (context.Plants.Any())
            {
                return;
            }

            // ==========================================
            // 3. Створення даних
            // ==========================================

            // --- Користувач ---
            var user = new User
            {
                Username = "admin",
                Email = "admin@greenhouse.com",
                FullName = "Administrator",
                PasswordHash = "secret_hash", // У реальному проекті тут має бути хешування
                Role = "Admin"
            };
            context.Users.Add(user);
            context.SaveChanges(); // Зберігаємо, щоб отримати ID

            // --- Рослини ---
            var tomatoes = new Plant { Name = "Помідори Черрі", Description = "Солодкий сорт для салатів" };
            var cucumbers = new Plant { Name = "Огірки", Description = "Гібридний сорт" };
            var basil = new Plant { Name = "Базилік", Description = "Фіолетовий запашний" };

            context.Plants.AddRange(tomatoes, cucumbers, basil);
            context.SaveChanges();

            // --- Сенсори ---
            var sensors = new Sensor[]
            {
                new Sensor 
                { 
                    Name = "Датчик #1 (Помідори)", 
                    PlantId = tomatoes.Id, 
                    UserId = user.Id, 
                    MinTemperature = 18, MaxTemperature = 28, 
                    MinHumidity = 60, MaxHumidity = 80,
                    MinLight = 200, MaxLight = 800
                },
                new Sensor 
                { 
                    Name = "Датчик #2 (Огірки)", 
                    PlantId = cucumbers.Id, 
                    UserId = user.Id, 
                    MinTemperature = 20, MaxTemperature = 30, 
                    MinHumidity = 70, MaxHumidity = 90,
                    MinLight = 150, MaxLight = 700
                },
                new Sensor 
                { 
                    Name = "Світловий сенсор (Базилік)", 
                    PlantId = basil.Id, 
                    UserId = user.Id, 
                    MinLight = 300, MaxLight = 800,
                    // Заповнюємо інші поля дефолтними значеннями, щоб не було помилок
                    MinTemperature = 15, MaxTemperature = 35,
                    MinHumidity = 40, MaxHumidity = 90
                }
            };

            context.Sensors.AddRange(sensors);
            context.SaveChanges();

            // --- Історія вимірювань (Measurements) ---
            // Генеруємо дані за останній тиждень для графіків
            var random = new Random();
            var measurements = new List<Measurement>();
            var startDate = DateTime.UtcNow.AddDays(-7); 

            foreach (var sensor in sensors)
            {
                // Генеруємо по 50 записів на кожен сенсор
                for (int i = 0; i < 50; i++) 
                {
                    measurements.Add(new Measurement
                    {
                        SensorId = sensor.Id,
                        UserId = user.Id,
                        Timestamp = startDate.AddHours(i * 3 + random.NextDouble()), // розкид у часі
                        Temperature = 20 + random.NextDouble() * 10, // 20...30 градусів
                        Humidity = 50 + random.NextDouble() * 30,    // 50...80%
                        Light = 100 + random.NextDouble() * 500,     // 100...600 люкс
                        Value = 0 // Якщо це поле використовується окремо
                    });
                }
            }

            context.Measurements.AddRange(measurements);
            context.SaveChanges();
        }
    }
}