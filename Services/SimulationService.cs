using SmartGreenhouse.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartGreenhouse.Services
{
    public class SimulationService
    {
        public List<Sensor> Sensors { get; private set; }
        public GreenhouseState State { get; private set; }

        public event Action<GreenhouseState> OnStateUpdated;

        public SimulationService()
        {
            Sensors = new List<Sensor>
            {
                new Sensor(SensorType.Temperature),
                new Sensor(SensorType.Humidity),
                new Sensor(SensorType.Light)
            };

            UpdateState();
        }

        private void UpdateState()
        {
            State = new GreenhouseState(
                Sensors[0].CurrentValue,
                Sensors[1].CurrentValue,
                Sensors[2].CurrentValue
            );
        }

        public async Task StartSimulationAsync(int intervalMs = 2000)
        {
            while (true)
            {
                foreach (var sensor in Sensors)
                    sensor.GenerateValue();

                UpdateState();
                OnStateUpdated?.Invoke(State);
                await Task.Delay(intervalMs);
            }
        }
    }
}
