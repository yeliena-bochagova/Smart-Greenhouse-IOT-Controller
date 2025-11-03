using System;

namespace SmartGreenhouse.Models
{
    public class LogRecord
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Device { get; set; }
        public double? Value { get; set; }
    }
}
