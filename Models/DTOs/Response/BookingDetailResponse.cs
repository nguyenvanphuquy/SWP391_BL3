namespace SWP391_BL3.Models.DTOs.Response
{
    public class BookingDetailResponse
    {
        // Thông tin booking
        public int BookingId { get; set; }
        public string? BookingCode { get; set; }
        public string? Purpose { get; set; }
        public string? Status { get; set; }
        public string NumberOfMenber {  get; set; }
        public string ApprovedAt { get; set; }
        public string RejectionReason { get; set; }
        public DateTime? CreateAt { get; set; }

        // Người đặt
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone {  get; set; }
        public string RoleName { get; set; }


        // Thông tin phòng
        public int FacilityId { get; set; }
        public string FacilityCode { get; set; }
        public int? Capacity { get; set; }
        public int? Floor { get; set; }
        public string Equipment { get; set; }

        // Thông tin loại phòng
        public string TypeName { get; set; }
        public string Description { get; set; }

        // Thông tin cơ sở
        public string CampusName { get; set; }
        public string Address { get; set; }
        public string? PhoneCampus { get; set; }
        // Thời gian đặt
        public DateOnly? BookingDate { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        //// Thông tin feedback
        //public string Comment { get; set; }
        //public int Rating { get; set; }

        // Thông tin checkin checkout
        public CheckInOutInfoResponse? CheckIn { get; set; }
        public CheckInOutInfoResponse? CheckOut { get; set; }

    }
}
