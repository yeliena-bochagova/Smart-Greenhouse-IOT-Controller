using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SmartGreenhouse.Web.Data;
using SmartGreenhouse.Web.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartGreenhouse.Web.Services
{
    public class DataGeneratorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Random _random = new Random();

        public DataGeneratorService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Чекаємо 10 секунд перед стартом, щоб база встигла створитися
            await Task.Delay(10000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        // 1. Беремо ВСІ сенсори, які є в базі (незалежно від юзера)
                        var sensors = await context.Sensors.ToListAsync(stoppingToken);

                        if (sensors.Any())
                        {
                            foreach (var sensor in sensors)
                            {
                                // Генеруємо реалістичні дані
                                double temp = 20 + (_random.NextDouble() * 10 - 5); // 15...25
                                double hum = 60 + (_random.NextDouble() * 20 - 10); // 50...70
                                double light = 500 + (_random.NextDouble() * 200 - 100); // 400...600

                                var measurement = new Measurement
                                {
                                    Timestamp = DateTime.UtcNow,
                                    SensorId = sensor.Id,
                                    UserId = sensor.UserId, // Прив'язуємо до власника сенсора
                                    
                                    // ЗАПОВНЮЄМО ВСІ ПОЛЯ
                                    Temperature = Math.Round(temp, 1),
                                    Humidity = Math.Round(hum, 1),
                                    Light = Math.Round(light, 0),
                                    Value = Math.Round(temp, 1) // Для сумісності
                                };

                                context.Measurements.Add(measurement);
                            }

                            await context.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Якщо помилка (наприклад, база заблокована), просто ігноруємо і пробуємо пізніше
                    Console.WriteLine($"Error generating data: {ex.Message}");
                }

                // Чекаємо 10 секунд перед наступним записом
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}