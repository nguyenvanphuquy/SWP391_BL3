namespace SWP391_BL3.Models.DTOs.Response
{
    public class FeedbackResponse
    {
        public int FeedbackId { get; set; }
        public string Comment { get; set; }
        public int? Rating { get; set; }
        public DateTime? CreateAt { get; set; }

        public string UserFullName { get; set; }
        public string FacilityCode { get; set; }
    }
}
