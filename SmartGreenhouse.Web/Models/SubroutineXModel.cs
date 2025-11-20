namespace SmartGreenhouse.Web.Models
{
    public class SubroutineXModel
    {
        // Вхідний параметр (наприклад для ручного введення)
        public string Input { get; set; } = "";

        // Результат обчислення субрутіну
        public string Output { get; set; } = "";

        // Данні сенсорів теплиці
        public double Temperature { get; set; } = 0.0;
        public double Humidity { get; set; } = 0.0;
        public double Illuminance { get; set; } = 0.0;
    }
}
