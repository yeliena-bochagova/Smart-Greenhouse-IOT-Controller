using SmartGreenhouse.Web.Models;
using SmartGreenhouse.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using System.Collections.Concurrent;

namespace SmartGreenhouse.Web.Services
{
    public class SensorService : ISensorService, IDisposable
    {
        private readonly ConcurrentDictionary<string, GreenhouseState> _states = new();
        private readonly List<string> _logs = new List<string>();
        private static readonly HttpClient _http = new HttpClient();
        private Timer _physicsTimer;
        private readonly IServiceProvider _serviceProvider;
        private readonly Random _random = new Random();

        public SensorService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            AddLog("System started.");

            // Таймер фізики кожні 5 секунд (щоб не забивати базу занадто швидко)
            _physicsTimer = new Timer(SimulatePhysics, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        public GreenhouseState GetState(string username)
        {
            return EnsureStateExists(username);
        }

        private GreenhouseState EnsureStateExists(string username)
        {
            if (_states.TryGetValue(username, out var state))
            {
                return state;
            }

            var newState = new GreenhouseState();
            // Спробувати отримати погоду при першому створенні
            Task.Run(() => FetchWeatherForState(newState, username));
            return _states.GetOrAdd(username, newState);
        }

        public List<string> GetLogs(string username) => _logs;

        // --- Керування кнопками ---
        public void ToggleHeater(string username)
        {
            var state = GetState(username);
            state.IsHeaterOn = !state.IsHeaterOn;
            if (state.IsHeaterOn)
            {
                state.IsVentilationOn = false;
                AddLog($"[{username}] Heater ON.");
            }
            else AddLog($"[{username}] Heater OFF.");
        }

        public void ToggleVentilation(string username)
        {
            var state = GetState(username);
            state.IsVentilationOn = !state.IsVentilationOn;
            if (state.IsVentilationOn)
            {
                state.IsHeaterOn = false;
                AddLog($"[{username}] Ventilation ON.");
            }
            else AddLog($"[{username}] Ventilation OFF.");
        }

        public void WaterPlants(string username)
        {
            var state = GetState(username);
            state.InsideHumidity += 5.0;
            if (state.InsideHumidity > 100) state.InsideHumidity = 100;
            AddLog($"[{username}] Plants watered (+5% Humidity).");
        }

        public void AddLight(string username)
        {
            var state = GetState(username);
            state.InsideLight += 50.0;
            AddLog($"[{username}] Light manually added (+50 lx).");
        }

        public async Task UpdateCoordinatesAsync(double lat, double lon, double volume, string username)
        {
            var state = GetState(username);
            state.Latitude = lat;
            state.Longitude = lon;
            state.Volume = volume;
            AddLog($"[{username}] Settings updated: Lat={lat}, Lon={lon}, Vol={volume}");
            await FetchWeatherForState(state, username);
        }

        // --- ФОНОВА ФІЗИКА І ЗБЕРЕЖЕННЯ В БД ---
        private void SimulatePhysics(object? state)
        {
            // Якщо ніхто не користується системою, states може бути порожнім.
            // У реальному додатку ми б завантажували користувачів з БД.
            if (_states.IsEmpty) return;

            foreach (var entry in _states)
            {
                string username = entry.Key;
                GreenhouseState userState = entry.Value;
                double dt = 2.0;

                // ===== 1. Логіка фізики (оновлення userState) =====
                
                // Температура
                double naturalTarget = userState.OutsideTemp ?? userState.InsideTemp;
                double targetTemp = naturalTarget;

                if (userState.IsHeaterOn)
                {
                    targetTemp = 28.0; // Гріємо до 28
                }
                else if (userState.IsVentilationOn)
                {
                    targetTemp = userState.OutsideTemp ?? 15.0; // Охолоджуємо до вуличної
                }

                // Плавна зміна температури
                if (userState.InsideTemp < targetTemp) userState.InsideTemp += 0.2;
                else if (userState.InsideTemp > targetTemp) userState.InsideTemp -= 0.1;

                // Джиттер (шум)
                userState.InsideTemp += (_random.NextDouble() - 0.5) * 0.1;
                userState.InsideTemp = Math.Round(userState.InsideTemp, 2);

                // Вологість
                if (userState.IsVentilationOn && userState.InsideHumidity > 40) 
                    userState.InsideHumidity -= 0.5;
                else if (userState.InsideHumidity < 90) 
                    userState.InsideHumidity += 0.1; // Природне зволоження від рослин
                
                userState.InsideHumidity = Math.Round(userState.InsideHumidity, 1);
                
                // Освітлення (проста логіка)
                userState.InsideLight = (userState.OutsideIlluminance ?? 0) * 0.8; // Частина світла проходить
                if (userState.InsideLight < 0) userState.InsideLight = 0;


                // ===== 2. Збереження у БД (ВИПРАВЛЕНО) =====
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var user = context.Users.FirstOrDefault(u => u.Username == username);

                    if (user != null)
                    {
                        // Знаходимо всі сенсори цього користувача
                        var sensors = context.Sensors.Where(s => s.UserId == user.Id).ToList();

                        foreach(var sensor in sensors)
                        {
                             var measurement = new Measurement
                            {
                                Timestamp = DateTime.UtcNow,
                                UserId = user.Id,
                                SensorId = sensor.Id, // Використовуємо реальний ID сенсора

                                // ЗАПИСУЄМО ДАНІ У ПРАВИЛЬНІ ПОЛЯ
                                Temperature = userState.InsideTemp + (_random.NextDouble() - 0.5), // Додаємо мікро-варіації для кожного сенсора
                                Humidity = userState.InsideHumidity + (_random.NextDouble() * 2 - 1),
                                Light = userState.InsideLight,
                                
                                Value = userState.InsideTemp // Дублюємо для сумісності
                            };
                            context.Measurements.Add(measurement);
                        }
                        
                        context.SaveChanges();
                    }
                }
            }
        }

        private void AddLog(string msg)
        {
            _logs.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {msg}");
            if (_logs.Count > 50) _logs.RemoveAt(_logs.Count - 1);
        }

        private async Task FetchWeatherForState(GreenhouseState state, string username)
        {
            try
            {
                // Заглушка, якщо координати 0
                if (state.Latitude == 0) state.Latitude = 50.45;
                if (state.Longitude == 0) state.Longitude = 30.52;

                string latStr = state.Latitude.ToString(CultureInfo.InvariantCulture);
                string lonStr = state.Longitude.ToString(CultureInfo.InvariantCulture);

                string url = $"https://api.open-meteo.com/v1/forecast?" +
                             $"latitude={latStr}&longitude={lonStr}" +
                             $"&current_weather=true" +
                             $"&hourly=relativehumidity_2m,shortwave_radiation" +
                             $"&timezone=UTC";

                using var resp = await _http.GetAsync(url);
                if (!resp.IsSuccessStatusCode) return;

                using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync());
                var root = doc.RootElement;

                if (root.GetProperty("current_weather").TryGetProperty("temperature", out var t))
                    state.OutsideTemp = t.GetDouble();

                if (root.TryGetProperty("hourly", out var hourly))
                {
                    if (hourly.TryGetProperty("relativehumidity_2m", out var rh) && rh.GetArrayLength() > 0)
                        state.OutsideHumidity = rh[0].GetDouble();

                    if (hourly.TryGetProperty("shortwave_radiation", out var sr) && sr.GetArrayLength() > 0)
                        state.OutsideIlluminance = sr[0].GetDouble() * 120.0; // Приблизна конвертація в люкси
                }

                AddLog($"[{username}] Weather updated: T={state.OutsideTemp}");
            }
            catch (Exception ex)
            {
                AddLog($"[{username}] Weather Error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _physicsTimer?.Dispose();
        }
    }
}