namespace SWP391_BL3.Models.DTOs.Request
{
    public class FeedbackRequest
    {
        public string Comment { get; set; }
        public int Rating { get; set; }

        public int UserId { get; set; }
        public int FacilityId { get; set; }
    }
}
