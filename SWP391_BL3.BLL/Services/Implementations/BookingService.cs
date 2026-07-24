using SWP391_BL3.DAL.Models.DTOs.Request;
using SWP391_BL3.DAL.Models.DTOs.Response;
using SWP391_BL3.DAL.Models.Entities;
using SWP391_BL3.DAL.Repositories.Interfaces;
using SWP391_BL3.BLL.Services.Interfaces;
namespace SWP391_BL3.BLL.Services.Implementations
{
    /*
     * ============================================
     * BUSINESS LOGIC - TR? L?I CùU H?I H?I ??NG
     * ============================================
     * 
     * Q12: T?i sao dùng Unit of Work thay vù inject DbContext?
     * A12: 
     * - Service khùng ph? thu?c tr?c ti?p EF Core / DbContext (?ùng t?ng BLL)
     * - IUnitOfWork qu?n lù transaction cross-repository t?p trung
     * - D? test vù thay ??i cùch l?u tr? sau nùy
     */
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ISlotRepository _slotRepository;
        private readonly IFacilityRepository _facilityRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationRepository _notificationRepository;
        private readonly ICheckinRepository _checkinRepository;
        private readonly ICheckoutRepository _checkoutRepository;
        public BookingService(IBookingRepository bookingRepository, ISlotRepository slotRepository, IFacilityRepository facilityRepository, IUnitOfWork unitOfWork, INotificationRepository notificationRepository, ICheckinRepository checkinRepository, ICheckoutRepository checkoutRepository)
        {
            _bookingRepository = bookingRepository;
            _slotRepository = slotRepository;
            _facilityRepository = facilityRepository;
            _unitOfWork = unitOfWork;
            _notificationRepository = notificationRepository;
            _checkinRepository = checkinRepository;
            _checkoutRepository = checkoutRepository;
        }
        /*
         * Q13: Logic x? lù conflict booking nhu th? nùo?
         * A13: 
         * - N?u dù cù booking "Approved" ? KHùNG cho d?t (throw exception)
         * - N?u cù booking "Pending" ho?c "Conflict" ? T?o booking m?i v?i status "Conflict"
         * - T?t c? booking conflict s? du?c admin duy?t, khi duy?t 1 booking thù t? d?ng reject cùc booking khùc
         * - Uu tiùn: First-come-first-served (khùng cù priority system)
         * 
         * Q14: Cù gi?i h?n s? lu?ng booking c?a user khùng?
         * A14: Cù gi?i h?n: M?i user ch? du?c d?t 1 booking trong cùng facility + date + slot
         * - Check trong HasUserBookedInSlot()
         * - Cù th? m? r?ng: Gi?i h?n t?ng s? booking trong tu?n/thùng
         */
        public BookingResponse CreateBooking(BookingRequest request)
        {
            // 1. VALIDATION CO B?N
            // Q15: Cù validate input d?y d? khùng?
            // A15: Cù validate co b?n (date, slot, facility, capacity)
            // Nùn thùm: FluentValidation package d? validate ph?c t?p hon
            if (request.BookingDate < DateOnly.FromDateTime(DateTime.Now))
            {
                throw new ArgumentException("Ngùy d?t phùng ph?i t? hùm nay tr? di");
            }

            var slot = _slotRepository.GetByNumber(request.SlotNumber);
            if (slot == null)
            {
                throw new ArgumentException($"Slot '{request.SlotNumber}' khùng t?n t?i.");
            }

            var facility = _facilityRepository.GetById(request.FacilityId);
            if (facility == null)
            {
                throw new ArgumentException($"Phùng '{request.FacilityId}' khùng t?n t?i.");
            }

            if (request.NumberOfMember > facility.Capacity)
            {
                throw new InvalidOperationException(
                    $"S? lu?ng ngu?i {request.NumberOfMember} vu?t quù s?c ch?a c?a phùng ({facility.Capacity} ngu?i).");
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
                    "B?n dù d?t l?ch trong khung gi? nùy. M?i ngu?i ch? du?c d?t m?t l?ch trong cùng khung gi?.");
            }

            /*
             * Q16: T?i sao dùng transaction?
             * A16: 
             * - ù?m b?o ACID: T?t c? operations (create booking, update conflict, create notification) 
             *   ph?i thùnh cùng ho?c cùng rollback
             * - Trùnh race condition: N?u 2 user cùng d?t cùng lùc, ch? 1 ngu?i thùnh cùng
             * - Isolation level: Default (Read Committed) - d? cho use case nùy
             */
            // 2. TRANSACTION d? d?m b?o consistency
            using var transaction = _unitOfWork.BeginTransaction();

            try
            {
                // 3. KI?M TRA CONFLICT
                var existingBookings = _bookingRepository
                    .GetBookingsByFacilityDateAndSlot(request.FacilityId, request.BookingDate, slot.SlotId);

                /*
                 * Q17: N?u nhi?u ngu?i d?t cùng slot, ai du?c uu tiùn?
                 * A17: 
                 * - Hi?n t?i: First-come-first-served (ai d?t tru?c du?c duy?t tru?c)
                 * - Admin cù th? ch?n b?t k? booking nùo d? duy?t
                 * - Khi duy?t 1 booking ? t? d?ng reject cùc booking conflict khùc
                 * - Cù th? c?i thi?n: Priority system (VIP user, urgent booking, etc.)
                 */
                // 3.1. N?u dù cù booking Approved ? KHùNG CHO ù?T
                var approvedBooking = existingBookings.FirstOrDefault(b => b.Status == "Approved");
                if (approvedBooking != null)
                {
                    throw new InvalidOperationException(
                        $"Phùng nùy dù cù l?ch d?t du?c duy?t (Booking #{approvedBooking.BookingCode}) trong khung gi? nùy.");
                }

                // 3.2. Xùc d?nh tr?ng thùi cho booking m?i
                // N?u cù booking khùc dang pending/conflict ? status = "Conflict"
                bool hasConflict = existingBookings.Any();
                string newBookingStatus = hasConflict ? "Conflict" : "Pending";

                // 4. T?O BOOKING M?I
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

                // 5. C?P NH?T CùC BOOKING CU (n?u cù conflict)
                if (hasConflict)
                {
                    // C?p nh?t T?T C? booking liùn quan (k? c? booking v?a t?o)
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
                    Title = "ù?t phùng thùnh cùng, ch? duy?t",
                    Message = $"B?n dù d?t phùng {created.Facility?.FacilityCode} vùo ngùy {created.BookingDate} - Slot {slot.SlotNumber}",
                    Status = "Unread",
                    Date = DateTime.Now,
                    UserId = created.UserId,
                    BookingId = created.BookingId
                };

                _notificationRepository.Add(noti);
                // 6. COMMIT TRANSACTION
                transaction.Commit();

                // 7. L?Y D? LI?U ù?Y ù?
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
            // 1. L?Y BOOKING Vù KI?M TRA
            var booking = _bookingRepository.GetByIdWithDetails(id);
            if (booking == null)
            {
                throw new ArgumentException($"Booking v?i ID {id} khùng t?n t?i.");
            }

            var originalStatus = booking.Status;

            // 2. TRANSACTION
            using var transaction = _unitOfWork.BeginTransaction();

            try
            {
                // 3. X? Lù THEO TR?NG THùI HI?N T?I C?A BOOKING

                // CASE 1: BOOKING ùANG Lù "PENDING" (ch? cù 1 booking duy nh?t)
                if (booking.Status == "Pending")
                {
                    if (request.Status == "Approved")
                    {
                        // Duy?t booking
                        booking.Status = "Approved";
                        booking.ApprovedByUserId = currentUserId;
                        booking.ApprovedAt = DateTime.Now;
                        booking.RejectionReason = null;

                        _notificationRepository.Add(new Notification
                        {
                            Title = "ù?t phùng dù du?c duy?t",
                            Message = $"Booking #{booking.BookingCode} c?a b?n dù du?c duy?t.",
                            Status = "Unread",
                            Date = DateTime.Now,
                            UserId = booking.UserId,
                            BookingId = booking.BookingId
                        });
                    }
                    else if (request.Status == "Rejected")
                    {
                        // T? ch?i booking
                        if (string.IsNullOrWhiteSpace(request.RejectionReason))
                        {
                            throw new ArgumentException("Lù do t? ch?i lù b?t bu?c khi t? ch?i booking.");
                        }

                        booking.Status = "Rejected";
                        booking.RejectionReason = request.RejectionReason;
                        booking.ApprovedByUserId = currentUserId;
                        booking.ApprovedAt = DateTime.Now;
                        _notificationRepository.Add(new Notification
                        {
                            Title = "ù?t phùng b? t? ch?i",
                            Message = $"Booking #{booking.BookingCode} b? t? ch?i. Lù do: {request.RejectionReason}",
                            Status = "Unread",
                            Date = DateTime.Now,
                            UserId = booking.UserId,
                            BookingId = booking.BookingId
                        });

                    }
                    else
                    {
                        // N?u g?i status khùc Approved/Rejected, gi? nguyùn Pending
                        booking.Status = "Pending";
                    }
                }
                // CASE 2: BOOKING ùANG Lù "CONFLICT" (cù nhi?u booking trùng)
                else if (booking.Status == "Conflict")
                {
                    if (request.Status == "Approved")
                    {
                        // 3.1. Duy?t booking hi?n t?i
                        booking.Status = "Approved";
                        booking.ApprovedByUserId = currentUserId;
                        booking.ApprovedAt = DateTime.Now;
                        booking.RejectionReason = null;

                        // 3.2. T? ù?NG T? CH?I T?T C? BOOKING CONFLICT KHùC
                        var conflictedBookings = _bookingRepository
                            .GetAllRelatedBookings(booking.FacilityId, booking.BookingDate, booking.SlotId, booking.BookingId);

                        foreach (var conflictedBooking in conflictedBookings)
                        {
                            conflictedBooking.Status = "Rejected";
                            conflictedBooking.RejectionReason = $"T? d?ng t? ch?i do xung d?t v?i booking #{booking.BookingCode} dù du?c duy?t";
                            conflictedBooking.ApprovedByUserId = currentUserId;
                            conflictedBooking.ApprovedAt = DateTime.Now;
                            conflictedBooking.UpdateAt = DateTime.Now;

                            _bookingRepository.Update(conflictedBooking);
                            _notificationRepository.Add(new Notification
                            {
                                Title = "ù?t phùng b? t? ch?i do xung d?t",
                                Message = $"Booking #{conflictedBooking.BookingCode} b? t? d?ng t? ch?i vù booking #{booking.BookingCode} dù du?c duy?t.",
                                Status = "Unread",
                                Date = DateTime.Now,
                                UserId = conflictedBooking.UserId,
                                BookingId = conflictedBooking.BookingId
                            });

                        }
                    }
                    else if (request.Status == "Rejected")
                    {
                        // T? ch?i 1 booking conflict (KHùNG t? d?ng duy?t booking khùc)
                        if (string.IsNullOrWhiteSpace(request.RejectionReason))
                        {
                            throw new ArgumentException("Lù do t? ch?i lù b?t bu?c khi t? ch?i booking.");
                        }

                        booking.Status = "Rejected";
                        booking.RejectionReason = request.RejectionReason;
                        booking.ApprovedByUserId = currentUserId;
                        booking.ApprovedAt = DateTime.Now;

                        // Cùc booking conflict khùc V?N GI? NGUYùN "Conflict"
                        // d? admin cù th? duy?t booking khùc sau nùy
                    }
                    else
                    {
                        // N?u g?i status khùc, gi? nguyùn Conflict
                        booking.Status = "Conflict";
                    }
                }
                // CASE 3: BOOKING ùù DUY?T/T? CH?I/H?Y
                else if (booking.Status == "Approved" || booking.Status == "Rejected" || booking.Status == "Cancelled")
                {
                    throw new InvalidOperationException(
                        $"Khùng th? c?p nh?t booking v?i tr?ng thùi '{booking.Status}'. " +
                        "Ch? cù th? c?p nh?t booking v?i tr?ng thùi Pending ho?c Conflict.");
                }
                // CASE 4: TR?NG THùI KHùC
                else
                {
                    throw new InvalidOperationException($"Tr?ng thùi '{booking.Status}' khùng du?c h? tr?.");
                }

                // 4. C?P NH?T TH?I GIAN
                if (booking.Status != originalStatus)
                {
                    booking.UpdateAt = DateTime.Now;
                }

                // 5. LUU THAY ù?I
                _bookingRepository.Update(booking);

                // 6. COMMIT TRANSACTION
                transaction.Commit();

                // 7. L?Y D? LI?U M?I NH?T
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
                    HasConflict = false, // Sau khi update thù khùng cùn conflict
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
         * Q20: Cù pagination cho GetAllBookings khùng?
         * A20: CHUA cù pagination.
         * - V?n d?: N?u cù 10,000 bookings ? load h?t vùo memory ? ch?m
         * - Nùn implement: Skip/Take ho?c cursor-based pagination
         * - Vù d?: GetAllBookings(int page = 1, int pageSize = 20)
         * - Ho?c: IQueryable d? client t? filter/sort/paginate
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
         * Q18: Check-in cù x? lù tru?ng h?p user check-in quù s?m/mu?n khùng?
         * A18: Cù x? lù:
         * - Ch? du?c check-in dùng ngùy d?t (BookingDate == Today)
         * - Ch? du?c check-in trong khung gi? slot (StartTime <= Now <= EndTime)
         * - Ch?n check-in nhi?u l?n (ki?m tra ExistsByBookingId)
         * - Cù th? c?i thi?n: Cho phùp check-in s?m 15 phùt, ho?c cù grace period
         * 
         * Q19: Cù penalty n?u user khùng check-in khùng?
         * A19: CHUA cù penalty system.
         * - Cù th? thùm: ùùnh d?u "No-show", gi?m priority cho booking sau
         * - Ho?c: T? d?ng cancel booking n?u khùng check-in sau 30 phùt
         */
        public BookingResponse CheckIn(CheckInOutRequest request)
        {
            var booking = _bookingRepository.GetBookingById(request.BookingId);

            if (booking == null)
                throw new ArgumentException("L?ch d?t khùng t?n t?i");

            if (booking.Status != "Approved")
                throw new InvalidOperationException("Ch? cù th? check-in cho l?ch dù du?c duy?t");

            // Validate: Ch? du?c check-in dùng ngùy d?t
            if (booking.BookingDate != DateOnly.FromDateTime(DateTime.Now))
                throw new InvalidOperationException("Ch? du?c check-in dùng ngùy d?t");

            if (booking.Slot == null)
                throw new InvalidOperationException("L?ch d?t chua cù slot");

            // Validate: Ch? du?c check-in trong khung gi? slot
            var now = TimeOnly.FromDateTime(DateTime.Now);
            if (now < booking.Slot.StartTime || now > booking.Slot.EndTime)
                throw new InvalidOperationException("Khùng n?m trong khung gi? d?t");

            // ? ch?n check-in nhi?u l?n
            if (_checkinRepository.ExistsByBookingId(booking.BookingId))
                throw new InvalidOperationException("L?ch d?t dù du?c check-in");

            // ? t?o record check-in
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
                throw new ArgumentException("L?ch d?t khùng t?n t?i");

            if (booking.Status != "CheckedIn")
                throw new InvalidOperationException("Ch? cù th? check-out sau khi check-in");

            if (booking.Slot == null)
                throw new InvalidOperationException("L?ch d?t chua cù slot");

            var now = TimeOnly.FromDateTime(DateTime.Now);
            if (now > booking.Slot.EndTime)
                throw new InvalidOperationException("Chua d?n th?i gian check-out");

            // ? ch?n check-out nhi?u l?n
            if (_checkoutRepository.ExistsByBookingId(booking.BookingId))
                throw new InvalidOperationException("L?ch d?t dù du?c check-out");

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
                throw new ArgumentException("L?ch d?t khùng t?n t?i");
            }
            if (booking.Status != "Approved" && booking.Status != "Pending" && booking.Status != "Conflict")
            {
                throw new InvalidOperationException("Ch? cù th? h?y cùc l?ch d?t dang ? tr?ng thùi Approved, Pending ho?c Conflict");
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