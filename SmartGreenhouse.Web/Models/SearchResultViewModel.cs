namespace SmartGreenhouse.Web.Models
{
    public class SearchResultViewModel
    {
        public DateTime Timestamp { get; set; }
        public string SensorName { get; set; } = string.Empty;
        public string PlantName { get; set; } = string.Empty;
        public double Value { get; set; }
    }
}
