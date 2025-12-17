using SWP391_BL3.Data;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Repositories.Interfaces;

namespace SWP391_BL3.Repositories.Implementations
{
    public class CheckinRepository : ICheckinRepository
    {
        private readonly FptBookingContext _context;
        public CheckinRepository(FptBookingContext context)
        {
            _context = context;
        }
        public void AddRange(List<Checkin> checkins)
        {
            _context.Checkins.AddRange(checkins);
            _context.SaveChanges();
        }
        public bool ExistsByBookingId(int bookingId)
        {
            return _context.Checkins.Any(x => x.BookingId == bookingId);
        }
    }
}
