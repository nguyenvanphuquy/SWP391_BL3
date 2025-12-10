namespace SWP391_BL3.Models.DTOs.Response
{
    public class FacilityDetailResponse
    {
            public int FacilityId { get; set; }
            public string FacilityCode { get; set; }
            public int? Capacity { get; set; }
            public int? Floor { get; set; }
            public string Equipment { get; set; }
            public string Status { get; set; }

            public int CampusId { get; set; }
            public string CampusName { get; set; }
            public string CampusAddress { get; set; }

            public int TypeId { get; set; }
            public string TypeName { get; set; }
            public string TypeDescription { get; set; }

            public List<SlotItem> Slots { get; set; } = new List<SlotItem>();

            public decimal? AverageRating { get; set; }
            public int TotalFeedback { get; set; }
            public List<FeedbackItem> RecentFeedback { get; set; } = new List<FeedbackItem>();
    }

        public class SlotItem
        {
            public int SlotId { get; set; }
            public int? SlotNumber { get; set; }
            public TimeOnly? StartTime { get; set; }
            public TimeOnly? EndTime { get; set; }
        }

        public class FeedbackItem
        {
            public string UserName { get; set; }
            public int? Rating { get; set; }
            public string Comment { get; set; }
            public DateTime? CreateAt { get; set; }
        }
}
