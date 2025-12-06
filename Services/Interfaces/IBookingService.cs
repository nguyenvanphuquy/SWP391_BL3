using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Models.DTOs.Request;

namespace SWP391_BL3.Services.Interfaces
{
    public interface IBookingService 
    {
        BookingResponse CreateBooking(BookingRequest request);
        BookingResponse UpdateBooking(int id, UpdateBookingRequest request);
        BookingResponse GetBooking(int id);
        List<BookingResponse> GetAllBookings();
        bool DeleteBooking(int id);
    }
}
