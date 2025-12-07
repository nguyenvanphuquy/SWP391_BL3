namespace SWP391_BL3.Models.DTOs.Response
{
    public class BookingStatsResponse
    {
        public int TotalBookings { get; set; }
        public double SuccessRate { get; set; }
        public string MostBookedFacilityType { get; set; }
    }
}
