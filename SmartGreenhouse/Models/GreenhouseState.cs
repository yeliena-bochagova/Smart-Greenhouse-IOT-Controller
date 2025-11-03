namespace SmartGreenhouse.Models
{
    public class GreenhouseState
    {
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Light { get; set; }

        public GreenhouseState(double temperature, double humidity, double light)
        {
            Temperature = temperature;
            Humidity = humidity;
            Light = light;
        }
    }
}
