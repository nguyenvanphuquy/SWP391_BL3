namespace SWP391_BL3.Models.DTOs.Response
{
    public class ListBookingUserResponse
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; }
        public int FacilityId { get; set; }
        public string FacilityCode { get; set; }
        public DateOnly? BookingDate { get; set; }
        public TimeOnly? Startime { get; set; }
        public TimeOnly? Endtime { get; set; }
        public string Purpose { get; set; }
        public string Status { get; set; }
    }
}
