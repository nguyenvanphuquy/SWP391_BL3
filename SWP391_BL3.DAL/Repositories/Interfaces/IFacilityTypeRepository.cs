using SWP391_BL3.DAL.Models.Entities;
using SWP391_BL3.DAL.Models.DTOs.Response;  
namespace SWP391_BL3.DAL.Repositories.Interfaces
{
    public interface IFacilityTypeRepository
    {
        IEnumerable<FacilityType> GetAll();
        FacilityType? GetById(int id);
        FacilityType? GetByName(string typeName);
        void Create(FacilityType facilityType);
        void Update(FacilityType facilityType);
        void Delete(FacilityType facilityType);
        List<ListTypeResponse> GetListType();
    }
}
