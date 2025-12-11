namespace SWP391_BL3.Models.DTOs.Request
{
    public class BookingRequest
    {
        public DateOnly BookingDate { get; set; }
        public string Purpose { get; set; }
        public int NumberOfMember { get; set; }

        public int UserId { get; set; }
        public int FacilityId { get; set; }
        public int SlotNumber { get; set; }
    }
}
