namespace SWP391_BL3.DAL.Models.DTOs.Response
{
    public class FacilityTypeResponse
    {
        public int TypeId { get; set; }
        public string? TypeName { get; set; }
        public string? Description { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
