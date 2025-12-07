namespace SWP391_BL3.Models.DTOs.Response
{
    public class BookingForUser
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; }
        public DateOnly? BookingDate { get; set; }
        public string Purpose { get; set; }
        public int? NumberOfMember { get; set; }
        public string Status { get; set; }
        public string FacilityCode { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string RejectionType { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
