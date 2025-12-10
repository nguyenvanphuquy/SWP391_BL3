using SWP391_BL3.Models.DTOs.Request;
using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Repositories.Implementations;
using SWP391_BL3.Repositories.Interfaces;
using SWP391_BL3.Services.Interfaces;
namespace SWP391_BL3.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ISlotRepository _slotRepository;
        private readonly IFacilityRepository _facilityRepository;
        public BookingService(IBookingRepository bookingRepository, ISlotRepository slotRepository, IFacilityRepository facilityRepository)
        {
            _bookingRepository = bookingRepository;
            _slotRepository = slotRepository;
            _facilityRepository = facilityRepository;
        }
        public BookingResponse CreateBooking(BookingRequest request)
        {
            // Kiểm tra BookingDate phải >= hôm nay
            if (request.BookingDate < DateOnly.FromDateTime(DateTime.Now))
            {
                throw new ArgumentException("Ngày đặt phòng phải từ hôm nay trở đi");
            }

            var slot = _slotRepository.GetByNumber(request.SlotNumber);
            if (slot == null)
            {
                throw new ArgumentException($"Slot '{request.SlotNumber}' not found.");
            }

            var facility = _facilityRepository.GetById(request.FacilityId);
            if (facility == null)
            {
                throw new ArgumentException($"Facility '{request.FacilityId}' không tồn tại.");
            }

            // Kiểm tra số lượng người
            if (request.NumberOfMember > facility.Capacity)
            {
                throw new InvalidOperationException(
                    $"Số lượng người {request.NumberOfMember} vượt quá sức chứa của phòng {facility.Capacity}.");
            }

            // ✅ SỬA: Kiểm tra tất cả booking xung đột (bao gồm Approved, Pending, Trùng)
            var conflictingBookings = _bookingRepository.GetAll()
                .Where(b => b.FacilityId == request.FacilityId
                    && b.BookingDate == request.BookingDate
                    && b.SlotId == slot.SlotId
                    && b.Status != "Cancelled"  // Chỉ loại trừ booking đã hủy
                    && b.Status != "Rejected")  // Và booking bị từ chối (nếu có)
                .ToList();

            bool hasConflict = conflictingBookings.Any();

            // Nếu có booking đã được Approved, KHÔNG CHO ĐẶT
            var hasApprovedBooking = conflictingBookings.Any(b => b.Status == "Approved");
            if (hasApprovedBooking)
            {
                throw new InvalidOperationException(
                    "Phòng này đã có lịch đặt được duyệt trong khung giờ này. Vui lòng chọn thời gian khác.");
            }

            // Tạo booking mới
            var booking = new Booking
            {
                BookingDate = request.BookingDate,
                Purpose = request.Purpose,
                NumberOfMenber = request.NumberOfMember,
                UserId = request.UserId,
                FacilityId = request.FacilityId,
                SlotId = slot.SlotId,
                Status = hasConflict ? "Trùng" : "Pending",
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            };

            var created = _bookingRepository.Create(booking);
            created = _bookingRepository.GetById(created.BookingId);
            created.BookingCode = "BK" + created.BookingId.ToString("D4");

            // Cập nhật tất cả các booking xung đột thành "Trùng"
            if (hasConflict)
            {
                foreach (var conflictingBooking in conflictingBookings)
                {
                    if (conflictingBooking.Status == "Pending")
                    {
                        conflictingBooking.Status = "Conflict";
                        conflictingBooking.UpdateAt = DateTime.Now;
                        _bookingRepository.Update(conflictingBooking);
                    }
                }
            }

            _bookingRepository.Update(created);

            return new BookingResponse
            {
                BookingId = created.BookingId,
                BookingCode = created.BookingCode,
                BookingDate = created.BookingDate,
                Purpose = created.Purpose,
                NumberOfMember = created.NumberOfMenber,
                Status = created.Status,
                UserFullName = created.User?.FullName ?? string.Empty,
                FacilityCode = created.Facility?.FacilityCode ?? string.Empty,
                HasConflict = hasConflict,
                ConflictingBookingCount = conflictingBookings.Count
            };
        }
        public BookingResponse UpdateBooking(int id, UpdateBookingRequest request, int currentUserId)
            {
                var booking = _bookingRepository.GetById(id);
                if (request.Status == "Approved")
                {
                    booking.Status = request.Status;
                    booking.ApprovedByUserId = currentUserId;
                    booking.ApprovedAt = DateTime.Now;
                    booking.RejectionReason = null; 
                }

                else if (request.Status == "Rejected")
                {
                    if (string.IsNullOrWhiteSpace(request.RejectionReason))
                    {
                        throw new ArgumentException("Rejection reason is required when rejecting a booking.");
                    }

                    booking.Status = request.Status;
                    booking.RejectionReason = request.RejectionReason;
                    booking.ApprovedByUserId = currentUserId; 
                    booking.ApprovedAt = DateTime.Now;
                }
                else
                {
                    booking.Status = request.Status;
                }

                booking.UpdateAt = DateTime.Now;

                _bookingRepository.Update(booking);
                booking = _bookingRepository.GetById(id);

                return new BookingResponse
                {
                    BookingId = booking.BookingId,
                    BookingCode = booking.BookingCode,
                    BookingDate = booking.BookingDate,
                    Purpose = booking.Purpose,
                    NumberOfMember = booking.NumberOfMenber,
                    Status = booking.Status,
                    ApprovedByUserId = booking.ApprovedByUserId,
                    ApprovedAt = booking.ApprovedAt,
                    RejectionReason = booking.RejectionReason,
                    UserFullName = booking.User.FullName,
                    FacilityCode = booking.Facility.FacilityCode,
                    UpdateAt = booking.UpdateAt
                };
            }

        public BookingResponse GetBooking(int id)
        {
            var booking = _bookingRepository.GetById(id);
            if (booking == null) return null;

            return new BookingResponse
            {
                BookingId = booking.BookingId,
                BookingCode = booking.BookingCode,
                BookingDate = booking.BookingDate,
                Purpose = booking.Purpose,
                NumberOfMember = booking.NumberOfMenber,
                Status = booking.Status,
                UserFullName = booking.User.FullName,
                FacilityCode = booking.Facility.FacilityCode
            };
        }

        public List<BookingResponse> GetAllBookings()
        {
            return _bookingRepository.GetAll()
                .Select(b => new BookingResponse
                {
                    BookingId = b.BookingId,
                    BookingCode = b.BookingCode,
                    BookingDate = b.BookingDate,
                    Purpose = b.Purpose,
                    NumberOfMember = b.NumberOfMenber,
                    Status = b.Status,
                    UserFullName = b.User.FullName,
                    FacilityCode = b.Facility.FacilityCode
                }).ToList();
        }

        public bool DeleteBooking(int id)
        {
            return _bookingRepository.Delete(id);
        }
        public List<BookingListResponse> GetBookingList()
        {
            return _bookingRepository.GetBookingList();
        }
        public BookingDetailResponse GetBookingDetail(int bookingId)
        {
            return _bookingRepository.GetBookingDetail(bookingId);
        }
        public List<ListBookingUserResponse> GetListBookingUsers(int userId)
        {
            return _bookingRepository.GetListBookingUsers(userId);
        }
        public BookingStatsResponse GetUserBookingStats(int userId)
        {
            return _bookingRepository.GetUserBookingStats(userId);
        }
    }
}