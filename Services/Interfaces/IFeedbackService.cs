using SWP391_BL3.Models.DTOs.Request;
using SWP391_BL3.Models.DTOs.Response;

namespace SWP391_BL3.Services.Interfaces
{
    public interface IFeedbackService
    {
        FeedbackResponse Create(FeedbackRequest request);
        FeedbackResponse Update(int id, UpdateFeedbackRequest request);
        bool Delete(int id);
        FeedbackResponse GetById(int id);
        IEnumerable<FeedbackResponse> GetAll();
        IEnumerable<FeedbackResponse> GetByFacility(int facilityId);
    }
}
