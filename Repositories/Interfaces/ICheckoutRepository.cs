using SWP391_BL3.Models.Entities;

namespace SWP391_BL3.Repositories.Interfaces
{
    public interface ICheckoutRepository
    {
        void AddRange(List<Checkout> checkins);
        bool ExistsByBookingId(int bookingId);
    }
}
