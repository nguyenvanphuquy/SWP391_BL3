using SWP391_BL3.DAL.Models.Entities;

namespace SWP391_BL3.DAL.Repositories.Interfaces
{
    public interface ICheckinRepository
    {
        void AddRange(List<Checkin> checkins);
        bool ExistsByBookingId(int bookingId);
    }
}
