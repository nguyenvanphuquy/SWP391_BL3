using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Models.Entities;

namespace SWP391_BL3.Repositories.Interfaces
{
    public interface IFeedbackRepository
    {
        Feedback Create(Feedback fb);
        Feedback GetByUserAndFacility(int userId, int facilityId);

        Feedback Update(Feedback fb);
        bool Delete(int id);
        Feedback GetByIdWithDetails(int id);
        Feedback GetById(int id);
        IEnumerable<Feedback> GetAll();
        IEnumerable<Feedback> GetByFacility(int facilityId);
        List<FeedbackListResponse> GetFeedbackList();
        FeedbackDetailResponse GetFeedbackDetail(int feedbackId);
    }
}
