using Microsoft.EntityFrameworkCore;
using SWP391_BL3.Data;
using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Repositories.Interfaces;
namespace SWP391_BL3.Repositories.Implementations
{
    public class BookingRepository : IBookingRepository
    {
        private readonly FptBookingContext _context;
        public BookingRepository(FptBookingContext context)
        {
            _context = context;
        }
        public Booking Create(Booking booking)
        {
            _context.Bookings.Add(booking);
            _context.SaveChanges();
            return booking;
        }

        public Booking Update(Booking booking)
        {
            _context.Bookings.Update(booking);
            _context.SaveChanges();
            return booking;
        }

        public Booking GetById(int id)
        {
            return _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Facility)
                .FirstOrDefault(b => b.BookingId == id);
        }

        public IEnumerable<Booking> GetAll()
        {
            return _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Facility)
                .ToList();
        }

        public bool Delete(int id)
        {
            var booking = _context.Bookings.Find(id);
            if (booking == null) return false;

            _context.Bookings.Remove(booking);
            _context.SaveChanges();
            return true;
        }
        public List<BookingListResponse> GetBookingList()
        {
            //string   slotTime = slot != null
            //? $"{slot.StartTime:hh\\:mm} - {slot.EndTime:hh\\:mm}"
            //: "N/A";
            var List = (from b in _context.Bookings
                        join u in _context.Users on b.UserId equals u.UserId
                        join f in _context.Facilities on b.FacilityId equals f.FacilityId
                        join sl in _context.Slots on b.SlotId equals sl.SlotId
                        select new BookingListResponse
                        {
                            BookingId = b.BookingId,
                            BookingCode = b.BookingCode,
                            FullName = u.FullName,
                            FacilityCode = f.FacilityCode,
                            BookingDate = b.BookingDate,
                            Purpose = b.Purpose,
                            Status = b.Status,
                        }).ToList();
            return List;
        }
    }
}
