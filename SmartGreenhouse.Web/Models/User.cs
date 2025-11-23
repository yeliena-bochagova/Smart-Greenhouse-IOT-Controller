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

        // Навігаційні властивості
        public ICollection<GreenhouseReading> Readings { get; set; } = new List<GreenhouseReading>();
        public GreenhouseSettings? Settings { get; set; }
    }
}
