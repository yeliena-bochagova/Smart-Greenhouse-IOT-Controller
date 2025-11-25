namespace SmartGreenhouse.Web.Models
{
    // Це клас, який зберігає стан твоєї теплиці
    public class GreenhouseState
    {
        public double InsideTemp { get; set; } = 20.0;
        public double InsideHumidity { get; set; } = 50.0;
        public double InsideLight { get; set; } = 200.0;

        public bool IsHeaterOn { get; set; }
        public bool IsVentilationOn { get; set; }
        
        public double Latitude { get; set; } = 50.27;
        public double Longitude { get; set; } = 30.31;
        public double Volume { get; set; } = 100.0;

        public double? OutsideTemp { get; set; }
        public double? OutsideHumidity { get; set; }
        public double? OutsideIlluminance { get; set; }
    }
}