using SmartGreenhouse.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace SmartGreenhouse.Web.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.Migrate();

            // User
            if (!context.Users.Any())
            {
                var user = new User
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    FullName = "Administrator",
                    PasswordHash = "",
                };

                context.Users.Add(user);
                context.SaveChanges();
            }

            var admin = context.Users.First();

            // Plant
            if (!context.Plants.Any())
            {
                context.Plants.Add(new Plant
                {
                    Name = "Default Plant",
                    Description = "Automatically added"
                });

                context.SaveChanges();
            }

            var plant = context.Plants.First();

            // Sensor
            if (!context.Sensors.Any())
            {
                context.Sensors.Add(new Sensor
                {
                    Name = "Main Sensor",
                    UserId = admin.Id,
                    PlantId = plant.Id,
                    MinTemperature = 18,
                    MaxTemperature = 30,
                    MinHumidity = 30,
                    MaxHumidity = 70,
                    MinLight = 100,
                    MaxLight = 1000
                });

                context.SaveChanges();
            }

            var sensor = context.Sensors.First();

            if (!context.Measurements.Any())
            {
                context.Measurements.Add(new Measurement
                {
                    SensorId = sensor.Id,
                    Timestamp = DateTime.Now,
                    Value = 42.5
                });

                context.SaveChanges();
            }
        }
    }
}
