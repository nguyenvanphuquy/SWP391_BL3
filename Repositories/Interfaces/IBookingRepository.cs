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
        List<Booking> GetBookingsByFacilityDateAndSlot(int facilityId, DateOnly bookingDate, int slotId);

        // Method tìm tất cả booking liên quan (dùng cho auto reject)
        List<Booking> GetAllRelatedBookings(int? facilityId, DateOnly? bookingDate, int? slotId, int excludeBookingId);

        // Method lấy slot
        Slot GetSlotByNumber(int slotNumber);

        Booking GetByIdWithDetails(int id);
        bool HasUserBookedInSlot(int userId, int facilityId, DateOnly bookingDate, int slotId);
        List<Booking> GetBookingsForFeedback(int userId, int facilityId, int maxDaysAgo = 30);
        List<BookingListResponse> GetBookingList();
        BookingDetailResponse GetBookingDetail(int bookingId);
        List<ListBookingUserResponse> GetListBookingUsers(int userId);
        BookingStatsResponse GetUserBookingStats(int userId);
    }
}
