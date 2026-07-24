using SWP391_BL3.DAL.Models.Entities;
using SWP391_BL3.DAL.Models.DTOs;
using SWP391_BL3.DAL.Models.DTOs.Response;
using SWP391_BL3.DAL.Models.DTOs.Request;
namespace SWP391_BL3.BLL.Services.Interfaces
{
    public interface IFacilityTypeService 
    {
        FacilityTypeResponse? GetById(int id);
        IEnumerable<FacilityTypeResponse> GetAll();
        FacilityTypeResponse?  Create(FacilityTypeRequest facilityTypeRequest);
        FacilityTypeResponse? Update(int id, FacilityTypeRequest facilityTypeRequest);
        bool Delete(int id);
        List<ListTypeResponse> GetListType();
    }
}
