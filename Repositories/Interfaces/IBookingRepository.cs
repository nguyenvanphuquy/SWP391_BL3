using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Models.Entities;

namespace SWP391_BL3.Repositories.Interfaces
{
    public interface IBookingRepository
    {
        Booking Create(Booking booking);
        Booking Update(Booking booking);
        Booking GetById(int id);
        IEnumerable<Booking> GetAll();
        bool Delete(int id);
        List<BookingListResponse> GetBookingList();
        BookingDetailResponse GetBookingDetail(int bookingId);
        List<ListBookingUserResponse> GetListBookingUsers(int userId);
        BookingStatsResponse GetUserBookingStats(int userId);
    }
}
