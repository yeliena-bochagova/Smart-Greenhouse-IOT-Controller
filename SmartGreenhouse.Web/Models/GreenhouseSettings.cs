namespace SmartGreenhouse.Web.Models
{
    public class GreenhouseSettings
    {
        public int Id { get; set; }   // Primary Key

        public double MinTemperature { get; set; }
        public double MaxTemperature { get; set; }

        public double MinHumidity { get; set; }
        public double MaxHumidity { get; set; }

        public double MinLight { get; set; }
        public double MaxLight { get; set; }

        // Foreign Key
        public int UserId { get; set; }
        public User User { get; set; } = null!;  
    }
}
