namespace SmartGreenhouse.Models
{
    public enum ActuatorType { Heater, WaterPump, LightSystem }

    public class Actuator
    {
        public ActuatorType Type { get; }
        public bool IsOn { get; private set; }

        public Actuator(ActuatorType type)
        {
            Type = type;
            IsOn = false;
        }

        public void TurnOn() => IsOn = true;
        public void TurnOff() => IsOn = false;
    }
}
