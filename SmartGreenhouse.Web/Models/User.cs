using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartGreenhouse.Web.Models
{
    public class User
    {
        public int Id { get; set; }   // Primary Key

        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; 
        public ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
        public ICollection<GreenhouseReading> Readings { get; set; } = new List<GreenhouseReading>();
        public GreenhouseSettings? Settings { get; set; }
    }
}