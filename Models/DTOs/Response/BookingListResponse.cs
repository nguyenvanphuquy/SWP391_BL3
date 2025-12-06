namespace SWP391_BL3.Models.DTOs.Response
{
    public class BookingListResponse
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; }
        public string FullName { get; set; }
        public string FacilityCode { get; set; }
        public TimeOnly? StartTime { get; set; }  
        public TimeOnly? EndTime { get; set; }
        public DateOnly? BookingDate { get; set; }
        public string Purpose { get; set; }
        public string Status { get; set; }
    }
}
