using SWP391_BL3.Models.Entities;

namespace SWP391_BL3.Repositories.Interfaces
{
    public interface IFeedbackRepository
    {
        Feedback Create(Feedback fb);
        Feedback Update(Feedback fb);
        bool Delete(int id);
        Feedback GetById(int id);
        IEnumerable<Feedback> GetAll();
        IEnumerable<Feedback> GetByFacility(int facilityId);
    }
}
