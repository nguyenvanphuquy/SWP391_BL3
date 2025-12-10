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
        public string? RejectionReason { get; set; }
        public int? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string RejectionType { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool HasConflict { get; set; }
        public int ConflictingBookingCount { get; set; }

    }
}
