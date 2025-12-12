namespace SWP391_BL3.Models.DTOs.Response
{
    public class NotificationResponse
    {
        public int NotificationId { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public string? Status { get; set; }
        public DateTime? Date { get; set; }
        public int? UserId { get; set; }
        public int? BookingId { get; set; }
        public string? BookingCode { get; set; }
        public string? UserFullName { get; set; }
    }
}
