using SWP391_BL3.Models.Entities;
namespace SWP391_BL3.Repositories.Interfaces
{
    public interface IFacilityTypeRepository
    {
        IEnumerable<FacilityType> GetAll();
        FacilityType? GetById(int id);
        void Create(FacilityType facilityType);
        void Update(FacilityType facilityType);
        void Delete(FacilityType facilityType);
    }
}
