using SWP391_BL3.DAL.Models.DTOs.Response;
using SWP391_BL3.DAL.Models.Entities;

namespace SWP391_BL3.DAL.Repositories.Interfaces
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
