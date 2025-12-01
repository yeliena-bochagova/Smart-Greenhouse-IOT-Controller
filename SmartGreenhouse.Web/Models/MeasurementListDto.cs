using System.Collections.Generic;

namespace SmartGreenhouse.Web.Models
{
    // DTO для відповіді API v2
    public class MeasurementListDto
    {
        public int Count { get; set; }
        public IEnumerable<Measurement> Items { get; set; }
    }
}