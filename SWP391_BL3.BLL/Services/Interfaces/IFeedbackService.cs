using SWP391_BL3.DAL.Models.DTOs.Request;
using SWP391_BL3.DAL.Models.DTOs.Response;

namespace SWP391_BL3.BLL.Services.Interfaces
{
    public interface IFeedbackService
    {
        FeedbackResponse Create(FeedbackRequest request);
        FeedbackResponse Update(int id, UpdateFeedbackRequest request);
        bool Delete(int id);
        FeedbackResponse GetById(int id);
        IEnumerable<FeedbackResponse> GetAll();
        IEnumerable<FeedbackResponse> GetByFacility(int facilityId);
        List<FeedbackListResponse> GetFeedbackList();
        FeedbackDetailResponse GetFeedbackDetail(int feedbackId);
    }
}
