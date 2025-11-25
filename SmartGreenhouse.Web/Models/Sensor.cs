namespace SmartGreenhouse.Web.Models
{
    public class Sensor
    {
        public int Id { get; set; }

        public int UserId { get; set; }      
        public User User { get; set; } = null!;

        public string Name { get; set; } = string.Empty;   
        public int? PlantId { get; set; }

        public Plant? Plant { get; set; }
                 

        public double MinTemperature { get; set; }  
        public double MaxTemperature { get; set; }
        public double MinHumidity { get; set; }
        public double MaxHumidity { get; set; }
        public double MinLight { get; set; }
        public double MaxLight { get; set; }
    }
}
