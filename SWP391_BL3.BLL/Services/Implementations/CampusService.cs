using SWP391_BL3.DAL.Models.DTOs.Response;
using SWP391_BL3.DAL.Repositories.Interfaces;
using SWP391_BL3.BLL.Services.Interfaces;
namespace SWP391_BL3.BLL.Services.Implementations
{
    public class CampusService : ICampusService
    {
        private readonly ICampusRepository _campusRepository;
        public CampusService(ICampusRepository campusRepository)
        {
            _campusRepository = campusRepository;
        }
        public IEnumerable<CampusResponse> GetAll()
        {
            var campuses = _campusRepository.GetAll();
            var campusResponses = campuses.Select(c => new CampusResponse
            {
                CampusId = c.CampusId,
                CampusName = c.CampusName,
                Phone = c.Phone,
                Status = c.Status,
                CreateAt = c.CreateAt
            });
            return campusResponses;
        }
    }
}
