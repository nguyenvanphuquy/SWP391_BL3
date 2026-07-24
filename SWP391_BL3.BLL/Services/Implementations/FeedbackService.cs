using SWP391_BL3.DAL.Models.DTOs.Request;
using SWP391_BL3.DAL.Models.DTOs.Response;
using SWP391_BL3.DAL.Models.Entities;
using SWP391_BL3.DAL.Repositories.Interfaces;
using SWP391_BL3.BLL.Services.Interfaces;

namespace SWP391_BL3.BLL.Services.Implementations
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;
        public FeedbackService(IFeedbackRepository feedbackRepository, IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
        {
            _feedbackRepository = feedbackRepository;
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
        }
        public FeedbackResponse Create(FeedbackRequest request)
        {
            // 1. Ki?m tra user d? feedback cho ph?ng n?y chua
            var existingFeedback = _feedbackRepository.GetByUserAndFacility(request.UserId, request.FacilityId);

            if (existingFeedback != null)
            {
                throw new InvalidOperationException("User d? feedback cho ph?ng n?y r?i. M?i user ch? du?c feedback 1 l?n.");
            }

            // 2. T?m booking ?? DUY?T (Approved) c?a user cho facility n?y
            // V? booking ph?i ?? DI?N RA (BookingDate <= h?m nay)
            var bookings = _bookingRepository.GetBookingsForFeedback(
                userId: request.UserId,
                facilityId: request.FacilityId,
                maxDaysAgo: 30 // Ch? cho feedback booking trong v?ng 30 ng?y g?n nh?t
            );

            if (!bookings.Any())
            {
                throw new InvalidOperationException(
                    "Kh?ng t?m th?y booking d? du?c duy?t v? d? di?n ra d? feedback. " +
                    "Ch? c? th? feedback booking d? du?c duy?t v? d? di?n ra.");
            }

            // 3. T?m booking G?N NH?T chua du?c feedback
            var bookingToUpdate = bookings
                .Where(b => b.Status != "Completed" && b.Status != "Feedbacked")
                .OrderByDescending(b => b.BookingDate)
                .FirstOrDefault();

            if (bookingToUpdate == null)
            {
                throw new InvalidOperationException("T?t c? booking c?a b?n cho ph?ng n?y d? du?c feedback.");
            }

            // 4. TRANSACTION
            using var transaction = _unitOfWork.BeginTransaction();

            try
            {
                // 5. T?o feedback
                var fb = new Feedback
                {
                    Comment = request.Comment,
                    Rating = request.Rating,
                    UserId = request.UserId,
                    FacilityId = request.FacilityId,
                    CreateAt = DateTime.Now
                };

                var saved = _feedbackRepository.Create(fb);

                // 6. C?P NH?T STATUS C?A BOOKING
                bookingToUpdate.Status = "Feedbacked"; // Ho?c "Feedbacked"
                bookingToUpdate.UpdateAt = DateTime.Now;
                _bookingRepository.Update(bookingToUpdate);

                // 7. COMMIT
                transaction.Commit();

                // 8. L?y d? li?u d?y d?
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

