using Xunit;
using SmartGreenhouse.Models;

namespace SmartGreenhouse.Tests
{
    public class TestModels
    {
        [Fact]
        public void Sensor_GeneratesValueWithinRange()
        {
            var tempSensor = new Sensor(SensorType.Temperature);
            var value = tempSensor.GenerateValue();
            Assert.InRange(value, 15, 30);
        }

        [Fact]
        public void Actuator_CanTurnOnAndOff()
        {
            var pump = new Actuator(ActuatorType.WaterPump);
            pump.TurnOn();
            Assert.True(pump.IsOn);
            pump.TurnOff();
            Assert.False(pump.IsOn);
        }

        [Fact]
        public void GreenhouseState_HoldsCorrectValues()
        {
            var state = new GreenhouseState(25, 60, 500);
            Assert.Equal(25, state.Temperature);
            Assert.Equal(60, state.Humidity);
            Assert.Equal(500, state.Light);
        }
    }
}
