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

        public MainWindow()
        {
            InitializeComponent();

            // Ініціалізація колекцій
            _sensors = new ObservableCollection<SensorData>();
            _logs = new ObservableCollection<string>();

            // Прив'язка до UI
            SensorsDataGrid.ItemsSource = _sensors;
            LogsListBox.ItemsSource = _logs;

            // Додаємо сенсори
            _sensors.Add(new SensorData("Температура", "°C", 20));
            _sensors.Add(new SensorData("Вологість", "%", 50));
            _sensors.Add(new SensorData("Освітлення", "lx", 200));

            // Налаштування таймера на 2 секунди
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += UpdateSensorData;
            _timer.Start();

            // Підключаємо події кнопок
            btnHeater.Click += BtnHeater_Click;
            btnWater.Click += BtnWater_Click;
            btnLight.Click += BtnLight_Click;
        }

        // Метод оновлення даних сенсорів
        public void UpdateSensorData(object? sender, EventArgs e)
        {
            foreach (var sensor in _sensors)
            {
                // Імітація випадкових змін даних
                switch (sensor.Name)
                {
                    case "Температура":
                        sensor.Value = _random.Next(15, 30);
                        break;
                    case "Вологість":
                        sensor.Value = _random.Next(30, 80);
                        break;
                    case "Освітлення":
                        sensor.Value = _random.Next(100, 500);
                        break;
                }
            }

            // Додаємо запис у лог
            _logs.Add($"[{DateTime.Now:T}] Дані сенсорів оновлено.");
        }

        // Обробники кнопок
        private void BtnHeater_Click(object sender, RoutedEventArgs e)
        {
            _logs.Add($"[{DateTime.Now:T}] Обігрівач перемкнено.");
        }

        private void BtnWater_Click(object sender, RoutedEventArgs e)
        {
            _logs.Add($"[{DateTime.Now:T}] Полив перемкнено.");
        }

        private void BtnLight_Click(object sender, RoutedEventArgs e)
        {
            _logs.Add($"[{DateTime.Now:T}] Освітлення перемкнено.");
        }
    }

    // Клас сенсора
    public class SensorData : INotifyPropertyChanged
    {
        private double _value;

        public string Name { get; set; }
        public string Unit { get; set; }

        public double Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public SensorData(string name, string unit, double value)
        {
            Name = name;
            Unit = unit;
            _value = value;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
