using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SmartGreenhouseConsole
{
    class Program
    {
        static List<Sensor> Sensors = new();
        static List<string> Logs = new();
        static HttpClient http = new HttpClient();

        static bool HeaterOn = false;
        static bool VentilationOn = false;

        static double GreenhouseVolume = 100;
        static double DeviceLat = 50.27;
        static double DeviceLon = 30.31;

        static double? OutsideTemp = null;
        static double? OutsideHumidity = null;
        static double? OutsideLight = null;

        static Random rng = new Random();
        static int? lastLoggedHour = null;

        static async Task Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            InitSensors();
            Log("System started.");

            // start weather loop
            _ = WeatherLoop();

            // start sensor update loop
            _ = SensorLoop();

            // main input loop
            await MainMenu();
        }

        static void InitSensors()
        {
            Sensors.Add(new Sensor("Temperature", "°C", 20));
            Sensors.Add(new Sensor("Humidity", "%", 50));
            Sensors.Add(new Sensor("Light", "lx", 200));

            // jitter initial values
            foreach (var s in Sensors)
                s.Value = s.Base + (rng.NextDouble() * 0.2 - 0.1);
        }

        static async Task MainMenu()
        {
            while (true)
            {
                PrintScreen();
                Console.WriteLine();
                Console.WriteLine("Commands:");
                Console.WriteLine(" 1 = Toggle Heater");
                Console.WriteLine(" 2 = Toggle Ventilation");
                Console.WriteLine(" 3 = Increase Humidity Base (+1)");
                Console.WriteLine(" 4 = Increase Light Base (+50)");
                Console.WriteLine(" 5 = Edit Coordinates");
                Console.WriteLine(" 6 = Edit Volume");
                Console.WriteLine(" L = Show logs");
                Console.WriteLine(" Q = Quit");
                Console.Write("> ");

                var key = Console.ReadKey().Key;
                Console.WriteLine();

                switch (key)
                {
                    case ConsoleKey.D1:
                        HeaterOn = !HeaterOn;
                        if (HeaterOn) VentilationOn = false;
                        Log($"Heater -> {(HeaterOn ? "ON" : "OFF")}");
                        break;

                    case ConsoleKey.D2:
                        VentilationOn = !VentilationOn;
                        if (VentilationOn) HeaterOn = false;
                        Log($"Ventilation -> {(VentilationOn ? "ON" : "OFF")}");
                        break;

                    case ConsoleKey.D3:
                        Sensors.First(s => s.Name == "Humidity").Base += 1;
                        Log("Humidity base increased by 1");
                        break;

                    case ConsoleKey.D4:
                        Sensors.First(s => s.Name == "Light").Base += 50;
                        Log("Light base increased by 50");
                        break;

                    case ConsoleKey.D5:
                        EditCoordinates();
                        break;

                    case ConsoleKey.D6:
                        EditVolume();
                        break;

                    case ConsoleKey.L:
                        ShowLogs();
                        break;

                    case ConsoleKey.Q:
                        return;
                }
            }
        }

        static void EditCoordinates()
        {
            Console.Write("Enter new coordinates (lat,lon): ");
            string input = Console.ReadLine();

            try
            {
                var parts = input.Split(',', ';');
                double lat = double.Parse(parts[0]);
                double lon = double.Parse(parts[1]);

                DeviceLat = lat;
                DeviceLon = lon;
                
                Log($"Coordinates updated → {lat:F3}, {lon:F3}");
            }
            catch
            {
                Log("Failed to update coordinates.");
            }
        }

        static void EditVolume()
        {
            Console.Write("Enter new greenhouse volume: ");
            if (double.TryParse(Console.ReadLine(), out double v) && v > 0)
            {
                GreenhouseVolume = v;
                Log($"Volume updated → {GreenhouseVolume}");
            }
            else Log("Invalid volume.");
        }

        static void PrintScreen()
        {
            Console.Clear();
            Console.WriteLine("===== SMART GREENHOUSE (Console Edition) =====\n");

            Console.WriteLine("** Inside Sensors **");
            foreach (var s in Sensors)
                Console.WriteLine($"{s.Name}: {s.Value:F2}{s.Unit}  (base {s.Base}{s.Unit})");

            Console.WriteLine("\n** Environment **");
            Console.WriteLine($"Coordinates: {DeviceLat:F3}, {DeviceLon:F3}");
            Console.WriteLine($"Volume: {GreenhouseVolume} m³");
            Console.WriteLine($"Outside Temp: {OutsideTemp:F2}°C");
            Console.WriteLine($"Outside Humidity: {OutsideHumidity:F2}%");
            Console.WriteLine($"Outside Light: {OutsideLight:F0} lx");

            Console.WriteLine("\n** Modes **");
            Console.WriteLine($"Heater:       {(HeaterOn ? "ON" : "OFF")}");
            Console.WriteLine($"Ventilation:  {(VentilationOn ? "ON" : "OFF")}");
        }

        static void ShowLogs()
        {
            Console.Clear();
            Console.WriteLine("===== LOGS =====\n");
            foreach (var l in Logs)
                Console.WriteLine(l);
            Console.WriteLine("\n(press any key)");
            Console.ReadKey();
        }

        static async Task SensorLoop()
        {
            while (true)
            {
                UpdateSensors();
                await Task.Delay(2000);
            }
        }

        static void UpdateSensors()
        {
            foreach (var s in Sensors)
            {
                if (s.Name == "Temperature")
                {
                    double target = VentilationOn
                        ? OutsideTemp ?? s.Base
                        : s.Base;

                    double diff = target - s.Value;
                    s.Value += diff * 0.1; // smoothing

                    if (Math.Abs(diff) < 0.1)
                        s.Value += (rng.NextDouble() - 0.5) * 0.05;
                }
                else if (s.Name == "Humidity")
                {
                    double target = VentilationOn
                        ? OutsideHumidity ?? s.Base
                        : s.Base;

                    double diff = target - s.Value;
                    s.Value += diff * 0.05;
                }
                else
                {
                    s.Value = s.Base + (rng.NextDouble() * 4 - 2);
                }
            }

            // hourly log
            var now = DateTime.Now;
            if (now.Minute == 0 && lastLoggedHour != now.Hour)
            {
                lastLoggedHour = now.Hour;
                Log($"Hourly: T={Sensors[0].Value:F1}, H={Sensors[1].Value:F1}, L={Sensors[2].Value:F0}");
            }
        }

        static async Task WeatherLoop()
        {
            while (true)
            {
                await FetchWeather();
                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }

        static async Task FetchWeather()
        {
            try
            {
                string url =
                    $"https://api.open-meteo.com/v1/forecast?latitude={DeviceLat}&longitude={DeviceLon}" +
                    $"&current_weather=true&hourly=relativehumidity_2m,shortwave_radiation&timezone=UTC";

                var resp = await http.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    Log("Weather fetch failed.");
                    return;
                }

                using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                var root = doc.RootElement;
                OutsideTemp = root.GetProperty("current_weather").GetProperty("temperature").GetDouble();

                var hourly = root.GetProperty("hourly");

                OutsideHumidity = hourly.GetProperty("relativehumidity_2m")[0].GetDouble();
                OutsideLight = hourly.GetProperty("shortwave_radiation")[0].GetDouble();

                Log("Weather updated.");
            }
            catch
            {
                Log("Weather error.");
            }
        }

        static void Log(string text)
        {
            string msg = $"[{DateTime.Now:HH:mm:ss}] {text}";
            Logs.Add(msg);
        }

        public class Sensor
        {
            public string Name;
            public string Unit;
            public double Base;
            public double Value;

            public Sensor(string name, string unit, double baseVal)
            {
                Name = name;
                Unit = unit;
                Base = baseVal;
                Value = baseVal;
            }
        }
    }
}
