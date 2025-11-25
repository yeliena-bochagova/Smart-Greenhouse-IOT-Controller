namespace SmartGreenhouse.Web.Models
{
    public class GreenhouseReading
    {
        public int Id { get; set; }   
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Light { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int SensorId { get; set; }
        public Sensor Sensor { get; set; }


        public double Value { get; set; } 
    }
}
