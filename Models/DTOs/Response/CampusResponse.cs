namespace SWP391_BL3.Models.DTOs.Response
{
    public class CampusResponse
    {
        public int CampusId { get; set; }
        public string? CampusName { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        public DateTime? CreateAt { get; set; }
    }
}
