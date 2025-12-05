namespace SWP391_BL3.Models.DTOs.Response
{
    public class FacilityResponse
    {
        public int FacilityId { get; set; }
        public string? FacilityCode { get; set; }
        public int? Capacity { get; set; }
        public int? Floor { get; set; }
        public string? Equipment { get; set; }
        public string? Status { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? CampusId { get; set; }
        public string CampusName { get; set; }
        public int? TypeId { get; set; }
        public string TypeName { get; set; }
    }
}
