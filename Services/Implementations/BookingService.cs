using Microsoft.EntityFrameworkCore;
using SWP391_BL3.Models.DTOs.Request;
using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Repositories.Implementations;
using SWP391_BL3.Repositories.Interfaces;
using SWP391_BL3.Services.Interfaces;
using SWP391_BL3.Data;
using System.Diagnostics.CodeAnalysis;
namespace SWP391_BL3.Services.Implementations
{
    /*
     * ============================================
     * BUSINESS LOGIC - TRẢ LỜI CÂU HỎI HỘI ĐỒNG
     * ============================================
     * 
     * Q12: Tại sao BookingService inject trực tiếp DbContext?
     * A12: 
     * - Cần quản lý transaction phức tạp (CreateBooking, UpdateBooking)
     * - Repository pattern không đủ để handle transaction cross-repository
     * - Có thể cải thiện: Tạo UnitOfWork pattern để quản lý transaction tập trung
     * - Hiện tại: OK cho project nhỏ, nhưng nên refactor khi scale lên
     */
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ISlotRepository _slotRepository;
        private readonly IFacilityRepository _facilityRepository;
        private readonly FptBookingContext _context; // Inject để quản lý transaction
        private readonly INotificationRepository _notificationRepository;
        private readonly ICheckinRepository _checkinRepository;
        private readonly ICheckoutRepository _checkoutRepository;
        public BookingService(IBookingRepository bookingRepository, ISlotRepository slotRepository, IFacilityRepository facilityRepository, FptBookingContext context, INotificationRepository notificationRepository, ICheckinRepository checkinRepository, ICheckoutRepository checkoutRepository)
        {
            _bookingRepository = bookingRepository;
            _slotRepository = slotRepository;
            _facilityRepository = facilityRepository;
            _context = context;
            _notificationRepository = notificationRepository;
            _checkinRepository = checkinRepository;
            _checkoutRepository = checkoutRepository;
        }
        /*
         * Q13: Logic xử lý conflict booking như thế nào?
         * A13: 
         * - Nếu đã có booking "Approved" → KHÔNG cho đặt (throw exception)
         * - Nếu có booking "Pending" hoặc "Conflict" → Tạo booking mới với status "Conflict"
         * - Tất cả booking conflict sẽ được admin duyệt, khi duyệt 1 booking thì tự động reject các booking khác
         * - Ưu tiên: First-come-first-served (không có priority system)
         * 
         * Q14: Có giới hạn số lượng booking của user không?
         * A14: Có giới hạn: Mỗi user chỉ được đặt 1 booking trong cùng facility + date + slot
         * - Check trong HasUserBookedInSlot()
         * - Có thể mở rộng: Giới hạn tổng số booking trong tuần/tháng
         */
        public BookingResponse CreateBooking(BookingRequest request)
        {
            // 1. VALIDATION CƠ BẢN
            // Q15: Có validate input đầy đủ không?
            // A15: Có validate cơ bản (date, slot, facility, capacity)
            // Nên thêm: FluentValidation package để validate phức tạp hơn
            if (request.BookingDate < DateOnly.FromDateTime(DateTime.Now))
            {
                throw new ArgumentException("Ngày đặt phòng phải từ hôm nay trở đi");
            }

            var slot = _slotRepository.GetByNumber(request.SlotNumber);
            if (slot == null)
            {
                throw new ArgumentException($"Slot '{request.SlotNumber}' không tồn tại.");
            }

            var facility = _facilityRepository.GetById(request.FacilityId);
            if (facility == null)
            {
                throw new ArgumentException($"Phòng '{request.FacilityId}' không tồn tại.");
            }

            if (request.NumberOfMember > facility.Capacity)
            {
                throw new InvalidOperationException(
                    $"Số lượng người {request.NumberOfMember} vượt quá sức chứa của phòng ({facility.Capacity} người).");
            }
            bool userAlreadyBooked = _bookingRepository.HasUserBookedInSlot(
                    request.UserId,
                    request.FacilityId,
                    request.BookingDate,
                    slot.SlotId
                );

            if (userAlreadyBooked)
            {
                throw new InvalidOperationException(
                    "Bạn đã đặt lịch trong khung giờ này. Mỗi người chỉ được đặt một lịch trong cùng khung giờ.");
            }

            /*
             * Q16: Tại sao dùng transaction?
             * A16: 
             * - Đảm bảo ACID: Tất cả operations (create booking, update conflict, create notification) 
             *   phải thành công hoặc cùng rollback
             * - Tránh race condition: Nếu 2 user cùng đặt cùng lúc, chỉ 1 người thành công
             * - Isolation level: Default (Read Committed) - đủ cho use case này
             */
            // 2. TRANSACTION để đảm bảo consistency
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // 3. KIỂM TRA CONFLICT
                var existingBookings = _bookingRepository
                    .GetBookingsByFacilityDateAndSlot(request.FacilityId, request.BookingDate, slot.SlotId);

                /*
                 * Q17: Nếu nhiều người đặt cùng slot, ai được ưu tiên?
                 * A17: 
                 * - Hiện tại: First-come-first-served (ai đặt trước được duyệt trước)
                 * - Admin có thể chọn bất kỳ booking nào để duyệt
                 * - Khi duyệt 1 booking → tự động reject các booking conflict khác
                 * - Có thể cải thiện: Priority system (VIP user, urgent booking, etc.)
                 */
                // 3.1. Nếu đã có booking Approved → KHÔNG CHO ĐẶT
                var approvedBooking = existingBookings.FirstOrDefault(b => b.Status == "Approved");
                if (approvedBooking != null)
                {
                    throw new InvalidOperationException(
                        $"Phòng này đã có lịch đặt được duyệt (Booking #{approvedBooking.BookingCode}) trong khung giờ này.");
                }

                // 3.2. Xác định trạng thái cho booking mới
                // Nếu có booking khác đang pending/conflict → status = "Conflict"
                bool hasConflict = existingBookings.Any();
                string newBookingStatus = hasConflict ? "Conflict" : "Pending";

                // 4. TẠO BOOKING MỚI
                var booking = new Booking
                {
                    BookingDate = request.BookingDate,
                    Purpose = request.Purpose,
                    NumberOfMenber = request.NumberOfMember,
                    UserId = request.UserId,
                    FacilityId = request.FacilityId,
                    SlotId = slot.SlotId,
                    Status = newBookingStatus,
                    CreateAt = DateTime.Now,
                    UpdateAt = DateTime.Now
                };

                var created = _bookingRepository.Create(booking);
                created.BookingCode = "BK" + created.BookingId.ToString("D4");

                // 5. CẬP NHẬT CÁC BOOKING CŨ (nếu có conflict)
                if (hasConflict)
                {
                    // Cập nhật TẤT CẢ booking liên quan (kể cả booking vừa tạo)
                    var allRelatedBookings = _bookingRepository
                        .GetBookingsByFacilityDateAndSlot(request.FacilityId, request.BookingDate, slot.SlotId);

                    foreach (var relatedBooking in allRelatedBookings)
                    {
                        if (relatedBooking.Status == "Pending" || relatedBooking.Status == "Conflict")
                        {
                            relatedBooking.Status = "Conflict";
                            relatedBooking.UpdateAt = DateTime.Now;
                            _bookingRepository.Update(relatedBooking);
                        }
                    }
                }
                var noti = new Notification
                {
                    Title = "Đặt phòng thành công, chờ duyệt",
                    Message = $"Bạn đã đặt phòng {created.Facility?.FacilityCode} vào ngày {created.BookingDate} - Slot {slot.SlotNumber}",
                    Status = "Unread",
                    Date = DateTime.Now,
                    UserId = created.UserId,
                    BookingId = created.BookingId
                };

                _notificationRepository.Add(noti);
                // 6. COMMIT TRANSACTION
                transaction.Commit();

                // 7. LẤY DỮ LIỆU ĐẦY ĐỦ
                created = _bookingRepository.GetByIdWithDetails(created.BookingId);

                return new BookingResponse
                {
                    BookingId = created.BookingId,
                    BookingCode = created.BookingCode,
                    BookingDate = created.BookingDate,
                    Purpose = created.Purpose,
                    NumberOfMember = created.NumberOfMenber,
                    Status = created.Status,
                    ApprovedByUserId = created.ApprovedByUserId,
                    ApprovedAt = created.ApprovedAt,
                    RejectionReason = created.RejectionReason,
                    UserFullName = created.User?.FullName ?? "N/A",
                    FacilityCode = created.Facility?.FacilityCode ?? "N/A",
                    SlotNumber = slot.SlotNumber,
                    CreatedAt = created.CreateAt,
                    UpdateAt = created.UpdateAt,
                    HasConflict = hasConflict,
                    ConflictingBookingCount = existingBookings.Count
                };
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
        public BookingResponse UpdateBooking(int id, UpdateBookingRequest request, int currentUserId)
        {
            // 1. LẤY BOOKING VÀ KIỂM TRA
            var booking = _bookingRepository.GetByIdWithDetails(id);
            if (booking == null)
            {
                throw new ArgumentException($"Booking với ID {id} không tồn tại.");
            }

            var originalStatus = booking.Status;

            // 2. TRANSACTION
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // 3. XỬ LÝ THEO TRẠNG THÁI HIỆN TẠI CỦA BOOKING

                // CASE 1: BOOKING ĐANG LÀ "PENDING" (chỉ có 1 booking duy nhất)
                if (booking.Status == "Pending")
                {
                    if (request.Status == "Approved")
                    {
                        // Duyệt booking
                        booking.Status = "Approved";
                        booking.ApprovedByUserId = currentUserId;
                        booking.ApprovedAt = DateTime.Now;
                        booking.RejectionReason = null;

                        _notificationRepository.Add(new Notification
                        {
                            Title = "Đặt phòng đã được duyệt",
                            Message = $"Booking #{booking.BookingCode} của bạn đã được duyệt.",
                            Status = "Unread",
                            Date = DateTime.Now,
                            UserId = booking.UserId,
                            BookingId = booking.BookingId
                        });
                    }
                    else if (request.Status == "Rejected")
                    {
                        // Từ chối booking
                        if (string.IsNullOrWhiteSpace(request.RejectionReason))
                        {
                            throw new ArgumentException("Lý do từ chối là bắt buộc khi từ chối booking.");
                        }

                        booking.Status = "Rejected";
                        booking.RejectionReason = request.RejectionReason;
                        booking.ApprovedByUserId = currentUserId;
                        booking.ApprovedAt = DateTime.Now;
                        _notificationRepository.Add(new Notification
                        {
                            Title = "Đặt phòng bị từ chối",
                            Message = $"Booking #{booking.BookingCode} bị từ chối. Lý do: {request.RejectionReason}",
                            Status = "Unread",
                            Date = DateTime.Now,
                            UserId = booking.UserId,
                            BookingId = booking.BookingId
                        });

                    }
                    else
                    {
                        // Nếu gửi status khác Approved/Rejected, giữ nguyên Pending
                        booking.Status = "Pending";
                    }
                }
                // CASE 2: BOOKING ĐANG LÀ "CONFLICT" (có nhiều booking trùng)
                else if (booking.Status == "Conflict")
                {
                    if (request.Status == "Approved")
                    {
                        // 3.1. Duyệt booking hiện tại
                        booking.Status = "Approved";
                        booking.ApprovedByUserId = currentUserId;
                        booking.ApprovedAt = DateTime.Now;
                        booking.RejectionReason = null;

                        // 3.2. TỰ ĐỘNG TỪ CHỐI TẤT CẢ BOOKING CONFLICT KHÁC
                        var conflictedBookings = _bookingRepository
                            .GetAllRelatedBookings(booking.FacilityId, booking.BookingDate, booking.SlotId, booking.BookingId);

                        foreach (var conflictedBooking in conflictedBookings)
                        {
                            conflictedBooking.Status = "Rejected";
                            conflictedBooking.RejectionReason = $"Tự động từ chối do xung đột với booking #{booking.BookingCode} đã được duyệt";
                            conflictedBooking.ApprovedByUserId = currentUserId;
                            conflictedBooking.ApprovedAt = DateTime.Now;
                            conflictedBooking.UpdateAt = DateTime.Now;

                            _bookingRepository.Update(conflictedBooking);
                            _notificationRepository.Add(new Notification
                            {
                                Title = "Đặt phòng bị từ chối do xung đột",
                                Message = $"Booking #{conflictedBooking.BookingCode} bị tự động từ chối vì booking #{booking.BookingCode} đã được duyệt.",
                                Status = "Unread",
                                Date = DateTime.Now,
                                UserId = conflictedBooking.UserId,
                                BookingId = conflictedBooking.BookingId
                            });

                        }
                    }
                    else if (request.Status == "Rejected")
                    {
                        // Từ chối 1 booking conflict (KHÔNG tự động duyệt booking khác)
                        if (string.IsNullOrWhiteSpace(request.RejectionReason))
                        {
                            throw new ArgumentException("Lý do từ chối là bắt buộc khi từ chối booking.");
                        }

                        booking.Status = "Rejected";
                        booking.RejectionReason = request.RejectionReason;
                        booking.ApprovedByUserId = currentUserId;
                        booking.ApprovedAt = DateTime.Now;

                        // Các booking conflict khác VẪN GIỮ NGUYÊN "Conflict"
                        // để admin có thể duyệt booking khác sau này
                    }
                    else
                    {
                        // Nếu gửi status khác, giữ nguyên Conflict
                        booking.Status = "Conflict";
                    }
                }
                // CASE 3: BOOKING ĐÃ DUYỆT/TỪ CHỐI/HỦY
                else if (booking.Status == "Approved" || booking.Status == "Rejected" || booking.Status == "Cancelled")
                {
                    throw new InvalidOperationException(
                        $"Không thể cập nhật booking với trạng thái '{booking.Status}'. " +
                        "Chỉ có thể cập nhật booking với trạng thái Pending hoặc Conflict.");
                }
                // CASE 4: TRẠNG THÁI KHÁC
                else
                {
                    throw new InvalidOperationException($"Trạng thái '{booking.Status}' không được hỗ trợ.");
                }

                // 4. CẬP NHẬT THỜI GIAN
                if (booking.Status != originalStatus)
                {
                    booking.UpdateAt = DateTime.Now;
                }

                // 5. LƯU THAY ĐỔI
                _bookingRepository.Update(booking);

                // 6. COMMIT TRANSACTION
                transaction.Commit();

                // 7. LẤY DỮ LIỆU MỚI NHẤT
                booking = _bookingRepository.GetByIdWithDetails(id);

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
                    UserFullName = booking.User?.FullName ?? "N/A",
                    FacilityCode = booking.Facility?.FacilityCode ?? "N/A",
                    SlotNumber = booking.Slot?.SlotNumber ?? 0,
                    CreatedAt = booking.CreateAt,
                    UpdateAt = booking.UpdateAt,
                    HasConflict = false, // Sau khi update thì không còn conflict
                    ConflictingBookingCount = 0
                };
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
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

        /*
         * Q20: Có pagination cho GetAllBookings không?
         * A20: CHƯA có pagination.
         * - Vấn đề: Nếu có 10,000 bookings → load hết vào memory → chậm
         * - Nên implement: Skip/Take hoặc cursor-based pagination
         * - Ví dụ: GetAllBookings(int page = 1, int pageSize = 20)
         * - Hoặc: IQueryable để client tự filter/sort/paginate
         */
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
        /*
         * Q18: Check-in có xử lý trường hợp user check-in quá sớm/muộn không?
         * A18: CÓ xử lý:
         * - Chỉ được check-in đúng ngày đặt (BookingDate == Today)
         * - Chỉ được check-in trong khung giờ slot (StartTime <= Now <= EndTime)
         * - Chặn check-in nhiều lần (kiểm tra ExistsByBookingId)
         * - Có thể cải thiện: Cho phép check-in sớm 15 phút, hoặc có grace period
         * 
         * Q19: Có penalty nếu user không check-in không?
         * A19: CHƯA có penalty system.
         * - Có thể thêm: Đánh dấu "No-show", giảm priority cho booking sau
         * - Hoặc: Tự động cancel booking nếu không check-in sau 30 phút
         */
        public BookingResponse CheckIn(CheckInOutRequest request)
        {
            var booking = _bookingRepository.GetBookingById(request.BookingId);

            if (booking == null)
                throw new ArgumentException("Lịch đặt không tồn tại");

            if (booking.Status != "Approved")
                throw new InvalidOperationException("Chỉ có thể check-in cho lịch đã được duyệt");

            // Validate: Chỉ được check-in đúng ngày đặt
            if (booking.BookingDate != DateOnly.FromDateTime(DateTime.Now))
                throw new InvalidOperationException("Chỉ được check-in đúng ngày đặt");

            if (booking.Slot == null)
                throw new InvalidOperationException("Lịch đặt chưa có slot");

            // Validate: Chỉ được check-in trong khung giờ slot
            var now = TimeOnly.FromDateTime(DateTime.Now);
            if (now < booking.Slot.StartTime || now > booking.Slot.EndTime)
                throw new InvalidOperationException("Không nằm trong khung giờ đặt");

            // ❌ chặn check-in nhiều lần
            if (_checkinRepository.ExistsByBookingId(booking.BookingId))
                throw new InvalidOperationException("Lịch đặt đã được check-in");

            // ✅ tạo record check-in
            var checkins = request.ImageUrls.Select(img => new Checkin
            {
                BookingId = booking.BookingId,
                ImageUrl = img,
                Comment = request.Comment,
                CreateAt = DateTime.Now
            }).ToList();

            _checkinRepository.AddRange(checkins);

            booking.Status = "CheckedIn";
            booking.UpdateAt = DateTime.Now;
            _bookingRepository.Update(booking);

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
        public BookingResponse CheckOut(CheckInOutRequest request)
        {
            var booking = _bookingRepository.GetBookingById(request.BookingId);

            if (booking == null)
                throw new ArgumentException("Lịch đặt không tồn tại");

            if (booking.Status != "CheckedIn")
                throw new InvalidOperationException("Chỉ có thể check-out sau khi check-in");

            if (booking.Slot == null)
                throw new InvalidOperationException("Lịch đặt chưa có slot");

            var now = TimeOnly.FromDateTime(DateTime.Now);
            if (now > booking.Slot.EndTime)
                throw new InvalidOperationException("Chưa đến thời gian check-out");

            // ❌ chặn check-out nhiều lần
            if (_checkoutRepository.ExistsByBookingId(booking.BookingId))
                throw new InvalidOperationException("Lịch đặt đã được check-out");

            var checkouts = request.ImageUrls.Select(img => new Checkout
            {
                BookingId = booking.BookingId,
                ImageUrl = img,
                Comment = request.Comment,
                CreateAt = DateTime.Now
            }).ToList();

            _checkoutRepository.AddRange(checkouts);

            booking.Status = "CheckedOut";
            booking.UpdateAt = DateTime.Now;
            _bookingRepository.Update(booking);
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
        public BookingResponse Cancel(int bookingId)
        {
            var booking = _bookingRepository.GetById(bookingId);
            if (booking == null)
            {
                throw new ArgumentException("Lịch đặt không tồn tại");
            }
            if (booking.Status != "Approved" && booking.Status != "Pending" && booking.Status != "Conflict")
            {
                throw new InvalidOperationException("Chỉ có thể hủy các lịch đặt đang ở trạng thái Approved, Pending hoặc Conflict");
            }
            booking.Status = "Cancelled";
            booking.UpdateAt = DateTime.Now;
            _bookingRepository.Update(booking);
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
    }
}