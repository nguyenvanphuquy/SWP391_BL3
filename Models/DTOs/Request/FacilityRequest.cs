using System.Reflection.Metadata;

namespace SWP391_BL3.Models.DTOs.Request
{
    public class FacilityRequest
    {
        public string FacilityCode { get; set; }
        public int Capacity { get; set; }
        public int Floor { get; set; }
        public string? Equipment { get; set; }
        public string? Status { get; set; }
        public string CampusName { get; set; }
        public string TypeName { get; set; }
    }
}
