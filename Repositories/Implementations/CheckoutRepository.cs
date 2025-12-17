using SWP391_BL3.Data;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Repositories.Interfaces;

namespace SWP391_BL3.Repositories.Implementations
{
    public class CheckoutRepository : ICheckoutRepository
    {
        private readonly FptBookingContext _context;
        public CheckoutRepository(FptBookingContext context)
        {
            _context = context;
        }
        public void AddRange(List<Checkout> checkouts)
        {
            _context.Checkouts.AddRange(checkouts);
            _context.SaveChanges();
        }
        public bool ExistsByBookingId(int bookingId)
        {
            return _context.Checkouts.Any(x => x.BookingId == bookingId);
        }
    }
}
