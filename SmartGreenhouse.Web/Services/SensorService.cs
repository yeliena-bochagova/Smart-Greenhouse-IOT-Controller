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

            // Таймер фізики кожні 2 секунди
            _physicsTimer = new Timer(SimulatePhysics, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
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

        // --- ФОНОВА ФІЗИКА ---
        private void SimulatePhysics(object? state)
        {
            foreach (var entry in _states)
            {
                string username = entry.Key;
                GreenhouseState userState = entry.Value;
                double dt = 2.0;

                // ===== Температура =====
                double naturalTarget = userState.OutsideTemp ?? userState.InsideTemp;
                double targetTemp;

                if (userState.IsVentilationOn)
                {
                    targetTemp = naturalTarget;
                }
                else if (userState.IsHeaterOn)
                {
                    double desiredHeat = 25.0;
                    double maxTarget = 30.0;
                    targetTemp = Math.Min(maxTarget, desiredHeat + (naturalTarget * 0.5));
                    targetTemp = Math.Max(targetTemp, userState.InsideTemp + 1.0);
                }
                else
                {
                    targetTemp = naturalTarget; // виправлено: прагнемо до зовнішньої температури
                }

                double diff = targetTemp - userState.InsideTemp;
                double rate = 0.005 * (100.0 / Math.Max(1.0, userState.Volume)); // сповільнено
                double alpha = 1.0 - Math.Exp(-rate * dt);
                double newValue = userState.InsideTemp + (diff * alpha);

                double changeDelta = newValue - userState.InsideTemp;
                double maxChange = userState.IsHeaterOn ? 0.2 : 0.05;
                maxChange = userState.IsVentilationOn ? 0.02 : maxChange;


                if (Math.Abs(changeDelta) > maxChange)
                {
                    newValue = userState.InsideTemp + Math.Sign(changeDelta) * maxChange;
                }

                userState.InsideTemp = Math.Round(newValue, 2);

                // Джиттер температури (завжди)
                double jitterTemp = (_random.NextDouble() * 0.001) - 0.0005;
                userState.InsideTemp = Math.Round(userState.InsideTemp * (1 + jitterTemp), 2);


                // ===== Вологість =====
                if (userState.InsideHumidity > 30)
                    userState.InsideHumidity -= 0.05;

                double targetHumidity = userState.IsVentilationOn
                    ? (userState.OutsideHumidity ?? 50.0)
                    : 50.0;

                // Джиттер вологості (завжди)
                double jitterHumidity = (_random.NextDouble() * 0.002) - 0.001;
                userState.InsideHumidity = Math.Round(userState.InsideHumidity * (1 + jitterHumidity), 1);

                userState.InsideHumidity = Math.Round(userState.InsideHumidity, 1);

                // Збереження у БД
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var user = context.Users.FirstOrDefault(u => u.Username == username);

                    if (user != null)
                    {
                        var measurement = new Measurement
                            {
                                Timestamp = DateTime.UtcNow,
                                SensorId = 1,                 
                                Value = userState.InsideTemp  
                            };

                        context.Measurements.Add(measurement);
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
                string latStr = state.Latitude.ToString(CultureInfo.InvariantCulture);
                string lonStr = state.Longitude.ToString(CultureInfo.InvariantCulture);

                string url = $"https://api.open-meteo.com/v1/forecast?" +
                             $"latitude={latStr}&longitude={lonStr}" +
                             $"&current_weather=true" +
                             $"&hourly=relativehumidity_2m,shortwave_radiation" +
                             $"&timezone=UTC";

                using var resp = await _http.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    AddLog($"[{username}] Weather API Error: {resp.StatusCode}");
                    return;
                }

                using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync());
                var root = doc.RootElement;

                if (root.GetProperty("current_weather").TryGetProperty("temperature", out var t))
                    state.OutsideTemp = t.GetDouble();

                var hourly = root.GetProperty("hourly");
                if (hourly.TryGetProperty("relativehumidity_2m", out var rh) && rh.GetArrayLength() > 0)
                    state.OutsideHumidity = rh[0].GetDouble();

                if (hourly.TryGetProperty("shortwave_radiation", out var sr) && sr.GetArrayLength() > 0)
                    state.OutsideIlluminance = sr[0].GetDouble() * 120.0;

                AddLog($"[{username}] Weather updated: T={state.OutsideTemp}°C, H={state.OutsideHumidity}%, L={state.OutsideIlluminance}lx");
            }
            catch (Exception ex)
            {
                AddLog($"[{username}] Weather API Exception: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _physicsTimer?.Dispose();
        }
    }
}
