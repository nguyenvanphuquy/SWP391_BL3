using SWP391_BL3.Models.Entities;

namespace SWP391_BL3.Repositories.Interfaces
{
    public interface ICheckinRepository
    {
        void AddRange(List<Checkin> checkins);
        bool ExistsByBookingId(int bookingId);
    }
}
