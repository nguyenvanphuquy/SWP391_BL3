namespace SWP391_BL3.Models.DTOs.Response
{
    public class FacilityListResponse
    {
        public int FacilityId { get; set; }
        public string FacilityCode { get;set; }
        public string CampusName { get; set; }
        public string TypeName { get; set; }
        public int? Capacity { get; set; }
        public int? Floors { get; set; }
        public string Equipment { get; set; }
        public string Status{ get; set; }


    }
}
