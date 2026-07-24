using SWP391_BL3.DAL.Models.DTOs.Request;
using SWP391_BL3.DAL.Models.DTOs.Response;

namespace SWP391_BL3.BLL.Services.Interfaces
{
    public interface IFacilityService
    {
        FacilityResponse? GetById(int id);
        IEnumerable<FacilityResponse> GetAll();
        FacilityResponse CreateFacility(FacilityRequest facilityRequest);
        FacilityResponse UpdateFacility(int id, FacilityRequest facilityRequestrequest);
        bool Delete(int id);
        List<FacilityListResponse> GetFacilityList();
        FacilityDetailResponse GetFacilityDetail(int facilityId);
    }
}
