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
                            StartTime = sl.StartTime,
                            EndTime = sl.EndTime,
                            Purpose = b.Purpose,
                            Status = b.Status,
                        }).ToList();
            return List;
        }
        public BookingDetailResponse GetBookingDetail(int bookingId)
        {
            var detail = (from b in _context.Bookings
                          join u in _context.Users on b.UserId equals u.UserId
                          join f in _context.Facilities on b.FacilityId equals f.FacilityId
                          join ft in _context.FacilityTypes on f.TypeId equals ft.TypeId
                          join c in _context.Campuses on f.CampusId equals c.CampusId
                          join sl in _context.Slots on b.SlotId equals sl.SlotId
                          where b.BookingId == bookingId
                          select new BookingDetailResponse
                          {
                              BookingId = b.BookingId,
                              BookingCode = b.BookingCode,
                              Status = b.Status,
                              CreateAt = b.CreateAt,
                              FullName = u.FullName,
                              FacilityName = f.FacilityCode,
                              FacilityType = ft.TypeName,
                              CampusName = c.CampusName,
                              Capacity = f.Capacity,
                              BookingDate = b.BookingDate,
                              StartTime = sl.StartTime,
                              EndTime = sl.EndTime,
                              Purpose = b.Purpose,
                          }).FirstOrDefault();
            return detail;
        }
        public List<ListBookingUserResponse> GetListBookingUsers(int userId)
        {
            var list = (from b in _context.Bookings
                        join f in _context.Facilities on b.FacilityId equals f.FacilityId
                        join sl in _context.Slots on b.SlotId equals sl.SlotId
                        where b.UserId == userId
                        select new ListBookingUserResponse
                        {
                            BookingId = b.BookingId,
                            BookingCode = b.BookingCode,
                            FacilityCode = f.FacilityCode,
                            BookingDate = b.BookingDate,
                            Startime = sl.StartTime,
                            Endtime = sl.EndTime,
                            Purpose = b.Purpose,
                            Status = b.Status,
                        }).ToList();
            return list;
        }
        public BookingStatsResponse GetUserBookingStats(int userId)
        {
            var bookings = _context.Bookings
                .Include(b => b.Facility)
                .Where(b => b.UserId == userId)
                .ToList();

            if (bookings.Count == 0)
            {
                return new BookingStatsResponse
                {
                    TotalBookings = 0,
                    SuccessRate = 0,
                    MostBookedFacilityType = "N/A"
                };
            }

            // Tổng số lần đặt phòng
            var total = bookings.Count;

            // Tỷ lệ đặt phòng thành công
            var approvedCount = bookings.Count(b => b.Status == "Approved");
            double successRate = Math.Round((approvedCount * 100.0) / total, 2);

            // Loại phòng đặt nhiều nhất
            var mostBooked = bookings
                .Where(b => b.Facility?.Type?.TypeName != null)  // lọc null an toàn
                .GroupBy(b => b.Facility.Type.TypeName)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "Unknown";

            return new BookingStatsResponse
            {
                TotalBookings = total,
                SuccessRate = successRate,
                MostBookedFacilityType = mostBooked
            };
        }

    }
}
