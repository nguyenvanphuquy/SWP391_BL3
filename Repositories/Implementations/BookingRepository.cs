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
        public List<Booking> GetBookingsByFacilityDateAndSlot(int facilityId, DateOnly bookingDate, int slotId)
        {
            return _context.Bookings
                .Where(b => b.FacilityId == facilityId
                    && b.BookingDate == bookingDate
                    && b.SlotId == slotId
                    && b.Status != "Cancelled"
                    && b.Status != "Rejected")
                .ToList();
        }

        public List<Booking> GetAllRelatedBookings(int? facilityId, DateOnly? bookingDate, int? slotId, int excludeBookingId)
        {
            return _context.Bookings
                .Where(b => b.FacilityId == facilityId
                    && b.BookingDate == bookingDate
                    && b.SlotId == slotId
                    && b.BookingId != excludeBookingId
                    && b.Status != "Cancelled"
                    && b.Status != "Rejected")
                .ToList();
        }

        public Slot GetSlotByNumber(int slotNumber)
        {
            return _context.Slots.FirstOrDefault(s => s.SlotNumber == slotNumber);
        }
        public Booking GetByIdWithDetails(int id)
        {
            return _context.Bookings
                .Include(b => b.User)          // Lấy thông tin User
                .Include(b => b.Facility)      // Lấy thông tin Facility
                .Include(b => b.Slot)          // Lấy thông tin Slot
                .FirstOrDefault(b => b.BookingId == id);
        }
        public bool HasUserBookedInSlot(int userId, int facilityId, DateOnly bookingDate, int slotId)
        {
            return _context.Bookings
                .Any(b => b.UserId == userId
                    && b.FacilityId == facilityId
                    && b.BookingDate == bookingDate
                    && b.SlotId == slotId
                    && b.Status != "Cancelled"  // Loại trừ đã hủy
                    && b.Status != "Rejected"); // Loại trừ bị từ chối
        }
        public List<Booking> GetBookingsForFeedback(int userId, int facilityId, int maxDaysAgo = 30)
        {
            var minDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-maxDaysAgo));
            var today = DateOnly.FromDateTime(DateTime.Now);

            return _context.Bookings
                .Where(b => b.UserId == userId
                    && b.FacilityId == facilityId
                    && b.Status == "Approved" // Chỉ booking đã duyệt
                    && b.BookingDate >= minDate // Trong vòng maxDaysAgo ngày
                    && b.BookingDate <= today) // Đã diễn ra (hôm nay hoặc trước đó)
                .OrderByDescending(b => b.BookingDate)
                .ToList();
        }
        public List<BookingListResponse> GetBookingList()
        {
            var List = (from b in _context.Bookings
                        join u in _context.Users on b.UserId equals u.UserId
                        join f in _context.Facilities on b.FacilityId equals f.FacilityId
                        join sl in _context.Slots on b.SlotId equals sl.SlotId
                        orderby  b.CreateAt descending
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
                        // Left join với Feedbacks: lấy feedback của user này cho facility này
                        join fb in _context.Feedbacks
                            on new { FacilityId = b.FacilityId, UserId = b.UserId }
                            equals new { fb.FacilityId, fb.UserId }
                            into fbGroup
                        from fb in fbGroup.DefaultIfEmpty()
                        where b.UserId == userId
                        orderby  b.CreateAt descending
                        select new ListBookingUserResponse
                        {
                            BookingId = b.BookingId,
                            BookingCode = b.BookingCode,
                            FacilityId = f.FacilityId,
                            FacilityCode = f.FacilityCode,
                            BookingDate = b.BookingDate,
                            Startime = sl.StartTime,
                            Endtime = sl.EndTime,
                            Purpose = b.Purpose,
                            Status = b.Status,

                            // Thông tin feedback nếu có
                            FeedbackId = fb != null ? fb.FeedbackId : 0,
                            Comment = fb != null ? fb.Comment : string.Empty,
                            Rating = fb != null ? fb.Rating : 0,
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

            var total = bookings.Count;

            var approvedCount = bookings.Count(b =>
                b.Status != null &&
                b.Status.Equals("approved", StringComparison.OrdinalIgnoreCase));
            double successRate = Math.Round((approvedCount * 100.0) / total, 2);

            var mostBooked = bookings
                .Where(b => b.Facility?.Type?.TypeName != null)  
                .GroupBy(b => b.Facility.Type.TypeName)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "";

            return new BookingStatsResponse
            {
                TotalBookings = total,
                SuccessRate = successRate,
                MostBookedFacilityType = mostBooked
            };
        }

    }
}
