using SWP391_BL3.Models.DTOs.Request;
using SWP391_BL3.Models.DTOs.Response;

namespace SWP391_BL3.Services.Interfaces
{
    public interface IFacilityService
    {
        FacilityResponse? GetById(int id);
        IEnumerable<FacilityResponse> GetAll();
        FacilityResponse CreateFacility(FacilityRequest facilityRequest);
        FacilityResponse UpdateFacility(int id, FacilityRequest facilityRequestrequest);
        bool Delete(int id);
        List<FacilityListResponse> GetFacilityList();
    }
}
