using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Net.Http;
using System.Text.Json;
using System.Threading;

namespace SmartGreenhouse.UI
{
    public partial class MainWindow : Window
    {
        // Колекція для сенсорів (зміни відображаються автоматично у DataGrid)
        private ObservableCollection<SensorData> _sensors;

        // Колекція для логів
        private ObservableCollection<string> _logs;

        // Таймер для оновлення даних
        private DispatcherTimer _timer;

        // Генератор випадкових чисел (імітуємо сенсори)
        private Random _random = new Random();

        // остання година, для якої вже був запис в лог (щоб логувати один раз на годину)
        private int? _lastLoggedHour = null;

        // дані для правої таблиці (ключ/значення)
        private ObservableCollection<EnvRecord> _envData = new();

        // координати пристрою (можна зробити налаштовуваними)
        private double _deviceLat = 50.27;
        private double _deviceLon = 30.31;

        // Параметри теплиці
        private double _greenhouseVolume = 100.0; // m^3, використовується у формулі швидкості
        private bool _ventilationOn = false;
        private bool _heaterOn = false;
        private double? _outsideTemperature = null; // °C, оновлюється періодично з мережі
        private double? _outsideHumidity = null;    // % відносної вологості зовні
        private double? _outsideIlluminance = null; // lx (приблизно з shortwave radiation -> lux)

        // HTTP-клієнт для зовнішньої погоди
        private static readonly HttpClient _http = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
            
            SensorData.OnBaseValueChanged += HandleBaseValueChanged;


            // Ініціалізація колекцій
            _sensors = new ObservableCollection<SensorData>();
            _logs = new ObservableCollection<string>();

            // Прив'язка до UI
            SensorsDataGrid.ItemsSource = _sensors;
            LogsListBox.ItemsSource = _logs;

            // Додаємо сенсори з базовими значеннями
            _sensors.Add(new SensorData("Температура", "°C", 20.0)); // base 20°C
            _sensors.Add(new SensorData("Вологість", "%", 50.0));    // base 50%
            _sensors.Add(new SensorData("Освітлення", "lx", 200.0)); // base 200 lx
            
            // Ініціалізація правої таблиці
        _envData = new ObservableCollection<EnvRecord>
    {
        new EnvRecord("Device coordinates", $"{_deviceLat:0.000},{_deviceLon:0.000}"),
        new EnvRecord("Greenhouse volume (m³)", _greenhouseVolume.ToString("0.##")),
        new EnvRecord("Outside temperature (°C)", "N/A"),
        new EnvRecord("Outside humidity (%)", "N/A"),
        new EnvRecord("Outside illuminance (lx)", "N/A")
        };
    EnvDataGrid.ItemsSource = _envData;
            UpdateEnvData(); // тепер не буде порожньо

            // Злегка джиттеримо початкові значення, щоб не стояли рівно на BaseValue
            foreach (var s in _sensors)
            {
                double jitter = (_random.NextDouble() * 0.002) - 0.001; // +/-0.1%
                s.Value = Math.Round(s.BaseValue * (1 + jitter), 2);
            }

            // Налаштування таймера на 2 секунди
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += UpdateSensorData;
            _timer.Start();

            // Підключаємо події кнопок
            btnHeater.Click += BtnHeater_Click;
            btnWater.Click += BtnWater_Click;       // btnWater — полив (змінює базову вологість)
            btnLight.Click += BtnLight_Click;
            btnVentilation.Click += BtnVentilation_Click;

            // зробити початковий запит негайно (fire-and-forget), потім цикл оновлення кожні 10 хв
            _ = FetchOutsideConditionsAsync(_deviceLat, _deviceLon);
            _ = FetchOutsideConditionsLoopAsync();

            _logs.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] System started. Volume={_greenhouseVolume} m³, outside temp={_outsideTemperature}°C, outside humidity={_outsideHumidity}%");
            UpdateModeButtonColors();

            EnvDataGrid.ItemsSource = _envData;
            UpdateEnvData(); // initial fill
        }

        private void UpdateEnvData()
        {
            // helper: set value by key
            void Set(string key, string value)
            {
                var r = _envData.FirstOrDefault(x => x.Key == key);
                if (r != null) r.Value = value;
            }

            Set("Device coordinates", $"{_deviceLat:0.000},{_deviceLon:0.000}");
            Set("Greenhouse volume (m³)", _greenhouseVolume.ToString("0.##"));
            Set("Outside temperature (°C)", _outsideTemperature.HasValue ? _outsideTemperature.Value.ToString("0.##") : "N/A");
            Set("Outside humidity (%)", _outsideHumidity.HasValue ? _outsideHumidity.Value.ToString("0.##") : "N/A");
            Set("Outside illuminance (lx)", _outsideIlluminance.HasValue ? Math.Round(_outsideIlluminance.Value, 0).ToString() : "N/A");
        }

        // === Обробка редагування комірок у правій таблиці (EnvDataGrid) ===
private async void EnvDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
{
    if (e.EditAction != DataGridEditAction.Commit)
        return;

    if (e.Row.Item is EnvRecord record)
    {
        string key = record.Key;
        string newValue = (e.EditingElement as TextBox)?.Text?.Trim() ?? "";

        if (key == "Device coordinates")
        {
            try
            {
                var parts = newValue.Split(',', ';');
                if (parts.Length == 2 &&
                    double.TryParse(parts[0], out double lat) &&
                    double.TryParse(parts[1], out double lon))
                {
                    _deviceLat = lat;
                    _deviceLon = lon;
                    _logs.Add($"[{DateTime.Now:HH:mm:ss}] Coordinates updated → lat={lat:F3}, lon={lon:F3}");
                    await FetchOutsideConditionsAsync(lat, lon);
                    UpdateEnvData();
                }
                else
                {
                    MessageBox.Show("Невірний формат координат. Використовуйте формат '50.27,30.31'", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    UpdateEnvData(); // повернути попереднє значення
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка оновлення координат: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else if (key == "Greenhouse volume (m³)")
        {
            if (double.TryParse(newValue, out double volume) && volume > 0)
            {
                _greenhouseVolume = volume;
                _logs.Add($"[{DateTime.Now:HH:mm:ss}] Volume updated → {_greenhouseVolume:F1} m³");
                UpdateEnvData();
            }
            else
            {
                MessageBox.Show("Введіть додатне число для об’єму теплиці.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                UpdateEnvData(); // повернути попереднє значення
            }
        }
    }
}


        // Простий запис для правої таблиці
        public class EnvRecord : INotifyPropertyChanged
        {
            public string Key { get; }
            private string _value;
            public string Value
            {
                get => _value;
                set
                {
                    if (_value != value)
                    {
                        _value = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                    }
                }
            }

            public EnvRecord(string key, string value)
            {
                Key = key;
                _value = value;
            }

            public event PropertyChangedEventHandler? PropertyChanged;
        }

        // Метод оновлення даних сенсорів
        // Значення плавно наближається до BaseValue; для температури і вологості враховується вентиляція і зовнішні умови

        public void UpdateSensorData(object? sender, EventArgs e)
        {
            double dt = _timer.Interval.TotalSeconds; // ~2s

            foreach (var sensor in _sensors)
            {
                if (sensor.Name == "Температура")
                {
                    // Якщо вентиляція увімкнена — ціллю є зовнішня температура (fallback -> BaseValue), інакше BaseValue
                    double target = _ventilationOn ? (_outsideTemperature ?? sensor.BaseValue) : sensor.BaseValue;

                    // Базова швидкість наближення (1/s)
                    double baseRate = 0.02;

                    // Фактор від об'єму: більша теплиця -> повільніше
                    double volumeFactor = 100.0 / Math.Max(1.0, _greenhouseVolume);

                    // Різниця між внутрішньою і цільовою температурою
                    double diff = Math.Abs(target - sensor.Value);

                    // diffFactor збільшує швидкість при більших розбіжностях
                    double diffFactor = 1.0 + Math.Min(5.0, diff / 5.0);

                    double ratePerSec;
                    double maxChangePerTick;

                    if (_ventilationOn)
                    {
                        // Вентиляція: загалом повільніше за нагрів, але залежить від величини різниці
                        // Коефіцієнти вибрано так: при великій різниці даємо відносно більший множник,
                        // при малій різниці — дуже малий множник (повільна адаптація)
                        double ventMultiplier;
                        if (diff >= 10.0) ventMultiplier = 0.8;   // відносно "швидко", але не миттєво
                        else if (diff > 5.0) ventMultiplier = 0.45; // середня швидкість
                        else ventMultiplier = 0.18;               // дуже повільно коли різниця мала

                        ratePerSec = baseRate * volumeFactor * diffFactor * ventMultiplier;

                        // Обмеження зміни за тик робимо помірним — вентиляція не повинна давати великі стрибки
                        // maxChangePerTick в градусах за один тик (2s)
                        maxChangePerTick = 0.3; // ~0.15 °C/s при 2s інтервалі
                    }
                    else
                    {
                        // Без вентиляції — зміни залежно від нагріву/охолодження (звичайна поведінка)
                        ratePerSec = baseRate * volumeFactor * diffFactor;
                        maxChangePerTick = 0.6; // швидкість нагріву/охолодження від обігрівача може бути більшою
                    }

                    // Якщо різниця дуже мала і немає вентиляції — додаємо невеликий джиттер, щоб не стояти зовсім статично
                    if (!_ventilationOn && Math.Abs(target - sensor.Value) < 0.02)
                    {
                        double jitter = (_random.NextDouble() * 0.002) - 0.001; // +/-0.1%
                        sensor.Value = Math.Round(sensor.Value * (1 + jitter), 2);
                    }
                    else
                    {
                        sensor.UpdateTowards(target, ratePerSec, dt, maxChangePerTick);
                    }
                }
                else if (sensor.Name == "Вологість")
                {
                    // Вологість: якщо вентиляція увімкнена — тягнемо до зовнішньої вологості (fallback -> BaseValue), інакше до BaseValue
                    double target = _ventilationOn ? (_outsideHumidity ?? sensor.BaseValue) : sensor.BaseValue;

                    double baseRateH = 0.008; // базова швидкість вологості (помірна)
                    double volumeFactor = 100.0 / Math.Max(1.0, _greenhouseVolume);
                    double diff = Math.Abs(target - sensor.Value);
                    double diffFactor = 1.0 + Math.Min(5.0, diff / 10.0);

                    // Коли вентиляція увімкнена, адаптація до зовнішньої вологості трохи швидша,
                    // але все одно обмежена об'ємом теплиці
                    double ventHMultiplier = _ventilationOn ? 0.6 : 0.0;
                    double ratePerSec = baseRateH * volumeFactor * diffFactor + ventHMultiplier;

                    // обмежимо зміну в % за тик
                    double maxChangePerTick = _ventilationOn ? 3.0 : 2.0; // при вентиляції можна змінюватися трішки швидше
                    sensor.UpdateTowards(target, ratePerSec, dt, maxChangePerTick);
                }
                else
                {
                    // Інші сенсори — маленький джиттер + рідкі спайки
                    double spikeChance = 0.06;
                    double r = _random.NextDouble();
                    if (r < spikeChance)
                    {
                        int sign = _random.Next(0, 2) == 0 ? -1 : 1;
                        double pct = 0.01 + _random.NextDouble() * 0.01; // 1-2%
                        sensor.Value = Math.Round(sensor.BaseValue * (1 + sign * pct), 2);
                    }
                    else
                    {
                        double jitter = (_random.NextDouble() * 0.002) - 0.001; // +/-0.1%
                        sensor.Value = Math.Round(sensor.BaseValue * (1 + jitter), 2);
                    }
                }
            }

            // Пишемо щогодинний запис лише один раз на годину, коли хвилина 00
            var now = DateTime.Now;
            if (now.Minute == 0 && _lastLoggedHour != now.Hour)
            {
                _lastLoggedHour = now.Hour;
                var t = _sensors.FirstOrDefault(s => s.Name == "Температура")?.Value ?? double.NaN;
                var h = _sensors.FirstOrDefault(s => s.Name == "Вологість")?.Value ?? double.NaN;
                var l = _sensors.FirstOrDefault(s => s.Name == "Освітлення")?.Value ?? double.NaN;
                string log = $"[{now:yyyy-MM-dd HH}:00] Hourly snapshot — T={t}{GetUnit("Температура")}, H={h}{GetUnit("Вологість")}, L={l}{GetUnit("Освітлення")}";
                _logs.Add(log);
            }
        }

        private string GetUnit(string sensorName)
        {
            return _sensors.FirstOrDefault(s => s.Name == sensorName)?.Unit ?? string.Empty;
        }

        // Кнопка нагріву — змінює BaseValue миттєво, actual Value наближається поступово
        private void BtnHeater_Click(object sender, RoutedEventArgs e)
        {
            var s = _sensors.FirstOrDefault(x => x.Name == "Температура");
            // toggle heater state; if turning on -> ensure ventilation off
            _heaterOn = !_heaterOn;
            if (_heaterOn)
            {
                _ventilationOn = false;
                if (s != null)
                {
                    // збережено попередню поведінку — при включенні нагрівача можна змінювати базу
                    s.ChangeBase(0.5); // підвищити базову температуру на 0.5°C
                    _logs.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Heater pressed — base temperature -> {s.BaseValue}{s.Unit}");
                }
            }
            UpdateModeButtonColors();
        }

        private void BtnWater_Click(object sender, RoutedEventArgs e)
        {
            var s = _sensors.FirstOrDefault(x => x.Name == "Вологість");
            if (s != null)
            {
                s.ChangeBase(1.0); // підвищити базову вологість на 1%
                _logs.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Water pressed — base humidity -> {s.BaseValue}{s.Unit}");
            }
            UpdateModeButtonColors();
        }

        // Кнопка вентиляції — перемикає вентиляцію (он/оф)
        private void BtnVentilation_Click(object sender, RoutedEventArgs e)
        {
            _ventilationOn = !_ventilationOn;
            if (_ventilationOn)
            {
                _heaterOn = false;
            }
            _logs.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Ventilation toggled -> {(_ventilationOn ? "ON" : "OFF")}, outsideTemp={_outsideTemperature}°C, outsideHumidity={_outsideHumidity}%");
            UpdateModeButtonColors();
        }

        private void BtnLight_Click(object sender, RoutedEventArgs e)
        {
            var s = _sensors.FirstOrDefault(x => x.Name == "Освітлення");
            if (s != null)
            {
                s.ChangeBase(50.0); // підвищити базове освітлення на 50 lx
                _logs.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Light pressed — base light -> {s.BaseValue}{s.Unit}");
            }
            UpdateModeButtonColors();
        }

        private void UpdateModeButtonColors()
        {
            // Heater ON -> heater green, ventilation red
            if (_heaterOn)
            {
                btnHeater.Background = Brushes.LightGreen;
                btnVentilation.Background = Brushes.LightCoral;
                return;
            }

            // Ventilation ON -> ventilation green, heater red
            if (_ventilationOn)
            {
                btnVentilation.Background = Brushes.LightGreen;
                btnHeater.Background = Brushes.LightCoral;
                return;
            }

            // none -> clear overridden backgrounds (return to theme/default)
            btnHeater.ClearValue(Button.BackgroundProperty);
            btnVentilation.ClearValue(Button.BackgroundProperty);
        }

        private void HandleBaseValueChanged(SensorData sensor)
{
    if (sensor.Name == "Температура")
    {
        _heaterOn = true;
        _ventilationOn = false;
        _logs.Add($"[{DateTime.Now:HH:mm:ss}] Base temperature manually changed → Heater ON");
    }
    else if (sensor.Name == "Вологість")
    {
        _heaterOn = false;
        _ventilationOn = false;
        _logs.Add($"[{DateTime.Now:HH:mm:ss}] Base humidity manually changed → Water mode ON");
    }
    UpdateModeButtonColors();
}


        // Фоновий цикл для оновлення зовнішньої температури і вологості кожні 10 хвилин
        private async Task FetchOutsideConditionsLoopAsync()
        {
            // дефолтні координати (можна робити налаштовуваними)
            double lat = _deviceLat;  // Київ
            double lon = _deviceLon;

            while (true)
            {
                try
                {
                    await FetchOutsideConditionsAsync(lat, lon);
                }
                catch
                {
                    // мовчимо про помилки мережі, залишаємо попереднє значення
                }

                // чекати 10 хвилин
                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }

        // Підтягуємо температуру та годинну вологість з Open-Meteo (hourly.relativehumidity_2m)
        private async Task FetchOutsideConditionsAsync(double lat, double lon)
        {
            try
            {
                string url = $"https://api.open-meteo.com/v1/forecast?" +
                             $"latitude={lat}&longitude={lon}" +
                             $"&current_weather=true" +
                             $"&hourly=relativehumidity_2m,shortwave_radiation" +
                             $"&timezone=UTC";

                using var resp = await _http.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    _logs.Add($"[{DateTime.Now:HH:mm:ss}] Weather fetch failed: HTTP {(int)resp.StatusCode}");
                    return;
                }

                using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);
                var root = doc.RootElement;

                // якщо це масив — беремо перший елемент
                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    root = root[0];
                }

                // ===== Поточна температура =====
                if (root.TryGetProperty("current_weather", out var cw) &&
                    cw.ValueKind == JsonValueKind.Object &&
                    cw.TryGetProperty("temperature", out var t) &&
                    t.TryGetDouble(out var tt))
                {
                    _outsideTemperature = tt;
                }

                // =====  Дані по годинах =====
                _outsideHumidity = null;
                _outsideIlluminance = null;

                if (root.TryGetProperty("hourly", out var hourly))
                {
                    // якщо це не об'єкт, пропускаємо
                    if (hourly.ValueKind != JsonValueKind.Object)
                    {
                        _logs.Add($"[{DateTime.Now:HH:mm:ss}] hourly is {hourly.ValueKind}, skipping detailed parse");
                    }
                    else
                    {
                        JsonElement rhArr, srArr, timeArr;
                        bool okRH = hourly.TryGetProperty("relativehumidity_2m", out rhArr);
                        bool okSR = hourly.TryGetProperty("shortwave_radiation", out srArr);
                        bool okT = hourly.TryGetProperty("time", out timeArr);

                        if (okRH && okT &&
                            rhArr.ValueKind == JsonValueKind.Array &&
                            timeArr.ValueKind == JsonValueKind.Array)
                        {
                            string hourKey = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:00");
                            int index = -1;
                            for (int i = 0; i < timeArr.GetArrayLength(); i++)
                            {
                                if (timeArr[i].GetString()?.StartsWith(hourKey) == true)
                                {
                                    index = i;
                                    break;
                                }
                            }

                            if (index >= 0)
                            {
                                if (index < rhArr.GetArrayLength() &&
                                    rhArr[index].TryGetDouble(out var rh))
                                    _outsideHumidity = rh;

                                if (okSR && srArr.ValueKind == JsonValueKind.Array &&
                                    index < srArr.GetArrayLength() &&
                                    srArr[index].TryGetDouble(out var srWm2))
                                    _outsideIlluminance = srWm2 * 120.0;
                            }
                        }
                    }
                }


                _logs.Add($"[{DateTime.Now:HH:mm:ss}] Weather updated: T={_outsideTemperature:F1}°C, H={_outsideHumidity:F0}%, SR={_outsideIlluminance:F0}");
                UpdateEnvData();

            }
            catch (Exception ex)
            {
                _logs.Add($"[{DateTime.Now:HH:mm:ss}] Weather fetch failed: {ex.Message}");
            }
        }
    }

    // Клас сенсора
    public class SensorData : INotifyPropertyChanged
    {
        private double _value;

        public string Name { get; set; }
        public string Unit { get; set; }

        // Базове (початкове) значення; від нього відбувається джиттер/відхилення

        public double BaseValue
        {
            get => _baseValue;
            set
            {
                if (Math.Abs(_baseValue - value) > 1e-6)
                {
                    _baseValue = value;
                    OnPropertyChanged(nameof(BaseValue));
                    OnBaseValueChanged?.Invoke(this); // 👉 повідомляємо головне вікно
                }
            }
        }

public static event Action<SensorData>? OnBaseValueChanged;

        private double _baseValue;

        public double Value
        {
            get => _value;
            set
            {
                if (Math.Abs(_value - value) > 1e-6)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public SensorData(string name, string unit, double baseValue)
        {
            Name = name;
            Unit = unit;
            BaseValue = baseValue;
            _value = baseValue;
        }

        // Змінити базове значення (наприклад, при натисканні кнопки)
        // Тепер не змінюємо фактичне Value — воно наближатиметься поступово у UpdateSensorData
        public void ChangeBase(double delta)
        {
            BaseValue = Math.Round(BaseValue + delta, 2);
            OnPropertyChanged(nameof(BaseValue));
        }

        // Оновити Value, наближаючи його до target з експоненціальним затуханням
        public void UpdateTowards(double target, double ratePerSecond, double dtSeconds, double maxChangePerTick = double.MaxValue)
        {
            // alpha = 1 - exp(-rate * dt)
            double alpha = 1.0 - Math.Exp(-Math.Max(0.0, ratePerSecond) * dtSeconds);
            double newValue = _value + (target - _value) * alpha;
            // обмежуємо зміну за тик, щоб уникнути різких стрибків
            double delta = newValue - _value;
            if (Math.Abs(delta) > maxChangePerTick)
            {
                newValue = _value + Math.Sign(delta) * maxChangePerTick;
            }
            Value = Math.Round(newValue, 2);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    
}