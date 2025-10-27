using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using SmartGreenhouse.UI;


namespace SmartGreenhouse.UI.Tests
{
    public class TestUI
    {
        [StaFact] // Вимагається для WPF
        public void SensorValues_ShouldUpdateInUI()
        {
            // Створюємо екземпляр MainWindow
            var window = new MainWindow();

            // Беремо перше значення сенсора перед оновленням
            var firstValue = window.SensorsDataGrid.Items.OfType<SensorData>().First().Value;

            // Викликаємо метод оновлення сенсорів вручну
            window.Dispatcher.Invoke(() => window.UpdateSensorData(null, null));

            // Беремо перше значення сенсора після оновлення
            var newValue = window.SensorsDataGrid.Items.OfType<SensorData>().First().Value;

            // Перевірка: значення змінилося
            Assert.NotEqual(firstValue, newValue);
        }

        [StaFact]
        public void ButtonClick_ShouldAddLog()
        {
            var window = new MainWindow();

            // Кількість записів у журналі до натискання
            var initialCount = window.LogsListBox.Items.Count;

            // Натискаємо кнопку обігрівача
            window.Dispatcher.Invoke(() => window.btnHeater.RaiseEvent(
                new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent)));

            // Перевірка: журнал збільшився на 1
            Assert.Equal(initialCount + 1, window.LogsListBox.Items.Count);
        }
    }
}
