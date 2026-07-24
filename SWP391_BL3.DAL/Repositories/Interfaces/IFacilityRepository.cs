using SWP391_BL3.DAL.Models.DTOs.Response;
using SWP391_BL3.DAL.Models.Entities;

namespace SWP391_BL3.DAL.Repositories.Interfaces
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
        List<FacilityListResponse> GetAllList();
        FacilityDetailResponse GetFacilityDetail(int facilityId);
    }
}
