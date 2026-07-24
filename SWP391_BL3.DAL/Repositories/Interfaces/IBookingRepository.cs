using SWP391_BL3.DAL.Models.DTOs.Response;
using SWP391_BL3.DAL.Models.Entities;

namespace SWP391_BL3.DAL.Repositories.Interfaces
{
    public interface IBookingRepository
    {
        Booking Create(Booking booking);
        Booking Update(Booking booking);
        Booking GetById(int id);
        Booking GetBookingById(int id);
        IEnumerable<Booking> GetAll();
        bool Delete(int id);
        List<Booking> GetBookingsByFacilityDateAndSlot(int facilityId, DateOnly bookingDate, int slotId);

        // Method tìm t?t c? booking liên quan (dùng cho auto reject)
        List<Booking> GetAllRelatedBookings(int? facilityId, DateOnly? bookingDate, int? slotId, int excludeBookingId);

        // Method l?y slot
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
