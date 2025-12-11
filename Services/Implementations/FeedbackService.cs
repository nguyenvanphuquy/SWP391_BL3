using Microsoft.EntityFrameworkCore;
using SWP391_BL3.Data;
using SWP391_BL3.Models.DTOs.Request;
using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Repositories.Implementations;
using SWP391_BL3.Repositories.Interfaces;
using SWP391_BL3.Services.Interfaces;

namespace SWP391_BL3.Services.Implementations
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly FptBookingContext _context;
        public FeedbackService(IFeedbackRepository feedbackRepository, IBookingRepository bookingRepository, FptBookingContext context)
        {
            _feedbackRepository = feedbackRepository;
            _bookingRepository = bookingRepository;
            _context = context;
        }
        public FeedbackResponse Create(FeedbackRequest request)
        {
            // 1. Kiểm tra user đã feedback cho phòng này chưa
            var existingFeedback = _feedbackRepository.GetByUserAndFacility(request.UserId, request.FacilityId);

            if (existingFeedback != null)
            {
                throw new InvalidOperationException("User đã feedback cho phòng này rồi. Mỗi user chỉ được feedback 1 lần.");
            }

            // 2. Tìm booking ĐÃ DUYỆT (Approved) của user cho facility này
            // Và booking phải ĐÃ DIỄN RA (BookingDate <= hôm nay)
            var bookings = _bookingRepository.GetBookingsForFeedback(
                userId: request.UserId,
                facilityId: request.FacilityId,
                maxDaysAgo: 30 // Chỉ cho feedback booking trong vòng 30 ngày gần nhất
            );

            if (!bookings.Any())
            {
                throw new InvalidOperationException(
                    "Không tìm thấy booking đã được duyệt và đã diễn ra để feedback. " +
                    "Chỉ có thể feedback booking đã được duyệt và đã diễn ra.");
            }

            // 3. Tìm booking GẦN NHẤT chưa được feedback
            var bookingToUpdate = bookings
                .Where(b => b.Status != "Completed" && b.Status != "Feedbacked")
                .OrderByDescending(b => b.BookingDate)
                .FirstOrDefault();

            if (bookingToUpdate == null)
            {
                throw new InvalidOperationException("Tất cả booking của bạn cho phòng này đã được feedback.");
            }

            // 4. TRANSACTION
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // 5. Tạo feedback
                var fb = new Feedback
                {
                    Comment = request.Comment,
                    Rating = request.Rating,
                    UserId = request.UserId,
                    FacilityId = request.FacilityId,
                    CreateAt = DateTime.Now
                };

                var saved = _feedbackRepository.Create(fb);

                // 6. CẬP NHẬT STATUS CỦA BOOKING
                bookingToUpdate.Status = "Feedbacked"; // Hoặc "Feedbacked"
                bookingToUpdate.UpdateAt = DateTime.Now;
                _bookingRepository.Update(bookingToUpdate);

                // 7. COMMIT
                transaction.Commit();

                // 8. Lấy dữ liệu đầy đủ
                saved = _feedbackRepository.GetByIdWithDetails(saved.FeedbackId);

                return ToResponse(saved);
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public FeedbackResponse Update(int id, UpdateFeedbackRequest req)
        {
            var fb = _feedbackRepository.GetById(id);
            if (fb == null) return null;

            fb.Comment = req.Comment;
            fb.Rating = req.Rating;

            var updated = _feedbackRepository.Update(fb);
            return ToResponse(updated);
        }

        public bool Delete(int id) => _feedbackRepository.Delete(id);

        public FeedbackResponse GetById(int id)
        {
            var fb = _feedbackRepository.GetById(id);
            return fb == null ? null : ToResponse(fb);
        }

        public IEnumerable<FeedbackResponse> GetAll()
            => _feedbackRepository.GetAll().Select(f => ToResponse(f));

        public IEnumerable<FeedbackResponse> GetByFacility(int facilityId)
            => _feedbackRepository.GetByFacility(facilityId).Select(f => ToResponse(f));

        private FeedbackResponse ToResponse(Feedback fb)
        {
            return new FeedbackResponse
            {
                FeedbackId = fb.FeedbackId,
                Comment = fb.Comment,
                Rating = fb.Rating,
                CreateAt = fb.CreateAt,
                UserFullName = fb.User?.FullName,
                FacilityCode = fb.Facility?.FacilityCode
            };
        }
        public List<FeedbackListResponse> GetFeedbackList()
        {
            return _feedbackRepository.GetFeedbackList();
        }
        public FeedbackDetailResponse GetFeedbackDetail(int feedbackId)
        {
            return _feedbackRepository.GetFeedbackDetail(feedbackId);
        }
    }
}

