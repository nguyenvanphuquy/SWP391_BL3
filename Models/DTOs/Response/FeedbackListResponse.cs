namespace SWP391_BL3.Models.DTOs.Response
{
    public class FeedbackListResponse
    {
        public int FeedbackId { get; set; }
        public string FullName { get; set; }
        public string FacilityCode { get; set; }
        public string Email { get; set; }
        public string Comments { get; set; }
        public int? Rating { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }
}
