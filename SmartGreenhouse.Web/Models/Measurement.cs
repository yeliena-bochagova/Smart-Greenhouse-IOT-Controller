using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartGreenhouse.Web.Models
{
    public class Measurement
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        public double Value { get; set; } 

        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Light { get; set; }

        public int SensorId { get; set; }
        [ForeignKey("SensorId")]
        public Sensor? Sensor { get; set; } 


        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}