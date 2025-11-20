using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading; // Для Таймера
using System.Threading.Tasks;
using System.Globalization; // Для крапок у числах
using SmartGreenhouse.Web.Models;

namespace SmartGreenhouse.Web.Services
{
    public class SensorService : ISensorService, IDisposable
    {
        private readonly GreenhouseState _state = new GreenhouseState();
        private readonly List<string> _logs = new List<string>();
        private static readonly HttpClient _http = new HttpClient();
        
        // Таймер для емуляції фізики (заміна DispatcherTimer з WPF)
        private Timer _physicsTimer;

        public SensorService()
        {
            AddLog("System started.");
            
            // Запускаємо початкове оновлення погоди
            _ = FetchWeather();

            // Запускаємо таймер: спрацьовує кожні 2 секунди (2000 мс)
            _physicsTimer = new Timer(SimulatePhysics, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
        }

        public GreenhouseState GetState() => _state;
        public List<string> GetLogs() => _logs;

        // --- Керування кнопками ---
        public void ToggleHeater()
        {
            _state.IsHeaterOn = !_state.IsHeaterOn;
            if (_state.IsHeaterOn)
            {
                _state.IsVentilationOn = false; // Нагрівач вимикає вентиляцію
                AddLog("Heater ON.");
            }
            else AddLog("Heater OFF.");
        }

        public void ToggleVentilation()
        {
            _state.IsVentilationOn = !_state.IsVentilationOn;
            if (_state.IsVentilationOn)
            {
                _state.IsHeaterOn = false; // Вентиляція вимикає нагрівач
                AddLog("Ventilation ON.");
            }
            else AddLog("Ventilation OFF.");
        }

        public void WaterPlants()
        {
            _state.InsideHumidity += 5.0; // Різкий скачок вологості при поливі
            if (_state.InsideHumidity > 100) _state.InsideHumidity = 100;
            AddLog("Plants watered (+5% Humidity).");
        }

        public void AddLight()
        {
            _state.InsideLight += 50.0;
            AddLog("Light manually added (+50 lx).");
        }

        public async Task UpdateCoordinatesAsync(double lat, double lon, double volume)
        {
            _state.Latitude = lat;
            _state.Longitude = lon;
            _state.Volume = volume;
            AddLog($"Settings updated: Lat={lat}, Lon={lon}, Vol={volume}");
            await FetchWeather();
        }

        // --- ФОНОВА ФІЗИКА (Те, чого не вистачало) ---
        private void SimulatePhysics(object? state)
        {
            // 1. Температура
            double targetTemp = _state.IsVentilationOn 
                ? (_state.OutsideTemp ?? 20.0) // Якщо вентиляція - прагнемо до вулиці
                : (_state.IsHeaterOn ? 30.0 : 15.0); // Якщо гріємо - до 30, якщо ні - падає до 15 (умовно)

            // Плавна зміна температури (наближення до цілі)
            double changeSpeed = 0.2; // Швидкість зміни
            if (_state.InsideTemp < targetTemp) _state.InsideTemp += changeSpeed;
            else if (_state.InsideTemp > targetTemp) _state.InsideTemp -= changeSpeed;

            // 2. Вологість (повільно падає, якщо не поливати)
            if (_state.InsideHumidity > 30) 
                _state.InsideHumidity -= 0.1;

            // 3. Округлення для краси
            _state.InsideTemp = Math.Round(_state.InsideTemp, 2);
            _state.InsideHumidity = Math.Round(_state.InsideHumidity, 1);
        }

        private void AddLog(string msg)
        {
            // Додаємо в початок списку
            _logs.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {msg}");
            // Тримаємо тільки останні 50 записів
            if (_logs.Count > 50) _logs.RemoveAt(_logs.Count - 1);
        }

        private async Task FetchWeather()
        {
            try
            {
                // ВАЖЛИВО: Використовуємо CultureInfo.InvariantCulture, щоб координати були з крапкою (50.27), а не комою
                string latStr = _state.Latitude.ToString(CultureInfo.InvariantCulture);
                string lonStr = _state.Longitude.ToString(CultureInfo.InvariantCulture);

                string url = $"https://api.open-meteo.com/v1/forecast?" +
                             $"latitude={latStr}&longitude={lonStr}" +
                             $"&current_weather=true" +
                             $"&hourly=relativehumidity_2m,shortwave_radiation" +
                             $"&timezone=UTC";

                using var resp = await _http.GetAsync(url);
                if (!resp.IsSuccessStatusCode) 
                {
                    AddLog($"Weather API Error: {resp.StatusCode}");
                    return;
                }

                using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync());
                var root = doc.RootElement;

                // Температура
                if (root.GetProperty("current_weather").TryGetProperty("temperature", out var t))
                {
                    _state.OutsideTemp = t.GetDouble();
                }

                // Інші показники (беремо першу годину для простоти)
                var hourly = root.GetProperty("hourly");
                if (hourly.TryGetProperty("relativehumidity_2m", out var rh) && rh.GetArrayLength() > 0)
                    _state.OutsideHumidity = rh[0].GetDouble();
                
                if (hourly.TryGetProperty("shortwave_radiation", out var sr) && sr.GetArrayLength() > 0)
                    _state.OutsideIlluminance = sr[0].GetDouble() * 120.0;

                AddLog($"Weather updated from API: T={_state.OutsideTemp}°C");
            }
            catch (Exception ex)
            {
                AddLog($"Weather API Exception: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _physicsTimer?.Dispose();
        }
    }
}