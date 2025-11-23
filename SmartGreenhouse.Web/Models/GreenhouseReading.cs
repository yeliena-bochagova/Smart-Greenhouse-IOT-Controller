namespace SmartGreenhouse.Web.Models
{
    public class GreenhouseReading
    {
        public int Id { get; set; }   // Primary Key
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Light { get; set; }

        // Foreign Key
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
