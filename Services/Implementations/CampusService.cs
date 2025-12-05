using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Repositories.Interfaces;
using SWP391_BL3.Services.Interfaces;
namespace SWP391_BL3.Services.Implementations
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
