using SWP391_BL3.Models.DTOs.Request;
using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Repositories.Interfaces;
using SWP391_BL3.Services.Interfaces;

namespace SWP391_BL3.Services.Implementations
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;
        public FeedbackService(IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }
        public FeedbackResponse Create(FeedbackRequest request)
        {
            var fb = new Feedback
            {
                Comment = request.Comment,
                Rating = request.Rating,
                UserId = request.UserId,
                FacilityId = request.FacilityId,
                CreateAt = DateTime.Now
            };

            var saved = _feedbackRepository.Create(fb);
            saved = _feedbackRepository.GetById(saved.FeedbackId);

            return ToResponse(saved);
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
    }
}

