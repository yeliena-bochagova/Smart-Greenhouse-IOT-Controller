namespace SmartGreenhouse.Web.Models
{
public class Plant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;  
    public string Description { get; set; } = string.Empty;
    public List<Sensor> Sensors { get; set; } = new List<Sensor>();
}
}
