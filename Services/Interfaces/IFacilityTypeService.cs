using SWP391_BL3.Models.Entities;
using SWP391_BL3.Models.DTOs;
using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Models.DTOs.Request;
namespace SWP391_BL3.Services.Interfaces
{
    public interface IFacilityTypeService 
    {
        FacilityTypeResponse? GetById(int id);
        IEnumerable<FacilityTypeResponse> GetAll();
        FacilityTypeResponse?  Create(FacilityTypeRequest facilityTypeRequest);
        FacilityTypeResponse? Update(int id, FacilityTypeRequest facilityTypeRequest);
        bool Delete(int id);
    }
}
