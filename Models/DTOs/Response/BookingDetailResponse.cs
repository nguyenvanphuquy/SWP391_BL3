namespace SWP391_BL3.Models.DTOs.Response
{
    public class BookingDetailResponse
    {
        // Thông tin booking
        public int BookingId { get; set; }
        public string BookingCode { get; set; }
        public string Status { get; set; }
        public DateTime? CreateAt { get; set; }

        // Người đặt
        public string FullName { get; set; }

        // Thông tin phòng
        public string FacilityName { get; set; }
        public string FacilityType { get; set; }
        public string CampusName { get; set; }
        public int? Capacity { get; set; }

        // Thời gian đặt
        public DateOnly? BookingDate { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        // Mục đích
        public string Purpose { get; set; }
    }
}
