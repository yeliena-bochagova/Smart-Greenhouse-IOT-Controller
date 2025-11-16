using System;
namespace SmartGreenhouse.Models
{
    public enum SensorType { Temperature, Humidity, Light }

    public class Sensor
    {
        public SensorType Type { get; }
        public double CurrentValue { get; private set; }
        private readonly Random _random = new Random();

        public Sensor(SensorType type)
        {
            Type = type;
            CurrentValue = GenerateValue();
        }

        public double GenerateValue()
        {
            switch (Type)
            {
                case SensorType.Temperature:
                    CurrentValue = Math.Round(_random.NextDouble() * 15 + 15, 2); // 15–30°C
                    break;
                case SensorType.Humidity:
                    CurrentValue = Math.Round(_random.NextDouble() * 50 + 40, 2); // 40–90%
                    break;
                case SensorType.Light:
                    CurrentValue = Math.Round(_random.NextDouble() * 700 + 300, 2); // 300–1000 lx
                    break;
            }
            return CurrentValue;
        }
    }
}
