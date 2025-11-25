namespace SmartGreenhouse.Web.Models
{
    public class Measurement
    {
        public int Id { get; set; }

        public int SensorId { get; set; }
        public DateTime Timestamp { get; set; }

        public double Value { get; set; }

        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Light { get; set; }

        public int UserId { get; set; }
    }
}
