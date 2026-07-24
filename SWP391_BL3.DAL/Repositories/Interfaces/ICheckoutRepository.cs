using SWP391_BL3.DAL.Models.Entities;

namespace SWP391_BL3.DAL.Repositories.Interfaces
{
    public interface ICheckoutRepository
    {
        void AddRange(List<Checkout> checkins);
        bool ExistsByBookingId(int bookingId);
    }
}
