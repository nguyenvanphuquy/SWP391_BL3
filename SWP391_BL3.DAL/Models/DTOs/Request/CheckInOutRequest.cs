namespace SWP391_BL3.DAL.Models.DTOs.Request
{
    public class CheckInOutRequest
    {
        public int BookingId { get; set; }
        public string? Comment { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }
}
