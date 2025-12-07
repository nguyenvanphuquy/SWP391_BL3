using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Models.Entities;

namespace SWP391_BL3.Repositories.Interfaces
{
    public interface IFacilityRepository
    {
        IEnumerable<Facility> GetAll();
        Facility? GetById(int id);
        Facility? GetByCode(string code);
        void Create(Facility facility);
        void Update(Facility facility);
        void Delete(Facility facility);
        List<FacilityListResponse> GetFacilityList();
    }
}
