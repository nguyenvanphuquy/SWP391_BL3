namespace SWP391_BL3.Models.DTOs.Response
{
    public class CheckInOutInfoResponse
    {
        public DateTime? CreateAt { get; set; }
        public string? Comment { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }
}
