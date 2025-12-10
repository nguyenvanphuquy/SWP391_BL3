namespace SWP391_BL3.Models.DTOs.Response
{
    public class GetListFacilityResponse
    {
        public int FacilityId { get; set; }
        public string FacilityName { get; set; }
        public string Description { get; set; }
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
