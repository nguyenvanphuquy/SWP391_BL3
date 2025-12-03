using SWP391_BL3.Services.Interfaces;
using SWP391_BL3.Repositories.Interfaces;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Models.DTOs.Request;
using SWP391_BL3.Models.DTOs.Response;
namespace SWP391_BL3.Services.Implementations
{
    public class FacilityTypeService : IFacilityTypeService
    {
        private readonly IFacilityTypeRepository _facilityTypeRepository;
        public FacilityTypeService(IFacilityTypeRepository facilityTypeRepository)
        {
            _facilityTypeRepository = facilityTypeRepository;
        }
        public IEnumerable<FacilityTypeResponse> GetAll()
        {
            var facilityTypes = _facilityTypeRepository.GetAll();
            return facilityTypes.Select(ft => new FacilityTypeResponse
            {
                TypeId = ft.TypeId,
                TypeName = ft.TypeName,
                Description = ft.Description
            });
        }
        public FacilityTypeResponse? GetById(int id)
        {
            var facilityType = _facilityTypeRepository.GetById(id);
            if (facilityType == null) return null;
            return new FacilityTypeResponse
            {
                TypeId = facilityType.TypeId,
                TypeName = facilityType.TypeName,
                Description = facilityType.Description
            };

        }
        public FacilityTypeResponse? Create(FacilityTypeRequest facilityTypeRequest)
        {
            var facilityType = new FacilityType
            {
                TypeName = facilityTypeRequest.TypeName,
                Description = facilityTypeRequest.Description,
            };
            _facilityTypeRepository.Create(facilityType);
            return new FacilityTypeResponse
            {
                TypeId = facilityType.TypeId,
                TypeName = facilityType.TypeName,
                Description = facilityType.Description
            };
        }
        public FacilityTypeResponse? Update(int id, FacilityTypeRequest facilityTypeRequest)
        {
            var facilityType = _facilityTypeRepository.GetById(id);
            if (facilityType == null) return null;
            facilityType.TypeName = facilityTypeRequest.TypeName;
            facilityType.Description = facilityTypeRequest.Description;
            _facilityTypeRepository.Update(facilityType);
            return new FacilityTypeResponse
            {
                TypeId = facilityType.TypeId,
                TypeName = facilityType.TypeName,
                Description = facilityType.Description
            };
        }
        public bool Delete(int id)
        {
            var facilityType = _facilityTypeRepository.GetById(id);
            if (facilityType == null) return false;
            _facilityTypeRepository.Delete(facilityType);
            return true;
        }
    }
}
