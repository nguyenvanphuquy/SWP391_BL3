namespace SWP391_BL3.Models.DTOs.Response
{
    public class BookingResponse
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; }
        public DateOnly? BookingDate { get; set; }
        public string Purpose { get; set; }
        public int? NumberOfMember { get; set; }
        public string Status { get; set; }

        public string UserFullName { get; set; }
        public string FacilityCode { get; set; }
    }
}
