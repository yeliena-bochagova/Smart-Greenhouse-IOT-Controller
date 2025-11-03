using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using SmartGreenhouse.UI;
using System.Threading;

namespace SmartGreenhouse.UI.Tests
{
    public class TestUI
    {
        private void RunInSta(Action action)
        {
            Exception? ex = null;
            var t = new Thread(() =>
            {
                try { action(); }
                catch (Exception e) { ex = e; }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
            if (ex != null) throw new Exception("Test failed in STA thread", ex);
        }

        [Fact]
        public void SensorValues_ShouldUpdateInUI()
        {
            RunInSta(() =>
            {
                var window = new MainWindow();
                var firstValue = window.SensorsDataGrid.Items.OfType<SensorData>().First().Value;
                window.UpdateSensorData(null, EventArgs.Empty);
                var newValue = window.SensorsDataGrid.Items.OfType<SensorData>().First().Value;
                Assert.NotEqual(firstValue, newValue);
                window.Close();
            });
        }

        [Fact]
        public void ButtonClick_ShouldAddLog()
        {
            RunInSta(() =>
            {
                var window = new MainWindow();
                var initialCount = window.LogsListBox.Items.Count;
                window.btnHeater.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Assert.Equal(initialCount + 1, window.LogsListBox.Items.Count);
                window.Close();
            });
        }
    }
}