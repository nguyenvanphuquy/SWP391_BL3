using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Models.DTOs.Request;

namespace SWP391_BL3.Services.Interfaces
{
    public interface IBookingService 
    {
        BookingResponse CreateBooking(BookingRequest request);
        BookingResponse UpdateBooking(int id, UpdateBookingRequest request, int currentUserId);
        BookingResponse GetBooking(int id);
        List<BookingResponse> GetAllBookings();
        bool DeleteBooking(int id);
        List<BookingListResponse> GetBookingList();
        BookingDetailResponse GetBookingDetail(int bookingId);
        List<ListBookingUserResponse> GetListBookingUsers(int userId);
        BookingStatsResponse GetUserBookingStats(int userId);
    }
}
