using System;
using System.Collections.Generic;

namespace SmartGreenhouse.Web.Models
{
    public class SearchViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public int? SensorId { get; set; }
        public List<Sensor> Sensors { get; set; } = new List<Sensor>();  

        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
    }
}
