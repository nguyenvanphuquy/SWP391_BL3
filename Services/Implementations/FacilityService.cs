using SWP391_BL3.Models.DTOs.Request;
using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Repositories.Implementations;
using SWP391_BL3.Repositories.Interfaces;
using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Services.Interfaces;

namespace SWP391_BL3.Services.Implementations
{
    public class FacilityService : IFacilityService
    {
        private readonly IFacilityRepository _facilityRepository;
        private readonly IFacilityTypeRepository _facilityTypeRepository;
        private readonly ICampusRepository _campusRepo;
        public FacilityService(IFacilityRepository facilityRepository, IFacilityTypeRepository facilityTypeRepository, ICampusRepository campusRepo)
        {
            _facilityRepository = facilityRepository;
            _facilityTypeRepository = facilityTypeRepository;
            _campusRepo = campusRepo;
        }
        public IEnumerable<FacilityResponse> GetAll()
        {
            var facility = _facilityRepository.GetAll();
            return facility.Select(f => new FacilityResponse
            {
                FacilityId = f.FacilityId,
                FacilityCode = f.FacilityCode,
                Capacity = f.Capacity,
                Floor = f.Floor,
                Equipment = f.Equipment,
                Status = f.Status,
                CreateAt = f.CreateAt,
                UpdateAt = f.UpdateAt,
            });
        }
        public FacilityResponse? GetById(int id)
        {
            var facility = _facilityRepository.GetById(id);
            if (facility == null) return null;
            return new FacilityResponse
            {
                FacilityId = facility.FacilityId,
                FacilityCode = facility.FacilityCode,
                Capacity = facility.Capacity,
                Floor = facility.Floor,
                Equipment = facility.Equipment,
                Status = facility.Status,
                CreateAt = facility.CreateAt,
                UpdateAt = facility.UpdateAt,
                CampusId = facility.CampusId,
                TypeId = facility.TypeId,
            };

        }
        public FacilityResponse CreateFacility(FacilityRequest facilityRequest)
        {
            // 1. Tìm Campus theo tên
            var campus = _campusRepo.GetByName(facilityRequest.CampusName);
            if (campus == null)
                throw new Exception("Campus not found: " + facilityRequest.CampusName);

            // 2. Tìm FacilityType theo tên
            var type = _facilityTypeRepository.GetByName(facilityRequest.TypeName);
            if (type == null)
                throw new Exception("Facility Type not found: " + facilityRequest.TypeName);

            // 3. Tạo Facility
            var facility = new Facility
            {
                FacilityCode = facilityRequest.FacilityCode,
                Capacity = facilityRequest.Capacity,
                Floor = facilityRequest.Floor,
                Equipment = facilityRequest.Equipment,
                Status = "Available",
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now,

                CampusId = campus.CampusId,   // Gán ID tự tìm được
                TypeId = type.TypeId
            };

            _facilityRepository.Create(facility);

            // 4. Trả về response chuẩn
            return new FacilityResponse
            {
                FacilityId = facility.FacilityId,
                FacilityCode = facility.FacilityCode,
                Capacity = facility.Capacity,
                Floor = facility.Floor,
                Equipment = facility.Equipment,
                Status = facility.Status,
                CampusName = facilityRequest.CampusName,
                TypeName = facilityRequest.TypeName
            };
        }

        public FacilityResponse UpdateFacility(int id, FacilityRequest facilityRequest)
        {
            // 1. Tìm facility
            var facility = _facilityRepository.GetById(id);
            if (facility == null)
                throw new Exception("Facility not found");

            // 2. Tìm Campus theo tên
            var campus = _campusRepo.GetByName(facilityRequest.CampusName);
            if (campus == null)
                throw new Exception("Campus not found: " + facilityRequest.CampusName);

            // 3. Tìm FacilityType theo tên
            var type = _facilityTypeRepository.GetByName(facilityRequest.TypeName);
            if (type == null)
                throw new Exception("Facility Type not found: " + facilityRequest.TypeName);

            // 4. Update
            facility.FacilityCode = facilityRequest.FacilityCode;
            facility.Capacity = facilityRequest.Capacity;
            facility.Floor = facilityRequest.Floor;
            facility.Equipment = facilityRequest.Equipment;
            facility.Status = facilityRequest.Status;
            facility.UpdateAt = DateTime.Now;

            facility.CampusId = campus.CampusId;
            facility.TypeId = type.TypeId;

            _facilityRepository.Update(facility);

            return new FacilityResponse
            {
                FacilityId = facility.FacilityId,
                FacilityCode = facility.FacilityCode,
                Capacity = facility.Capacity,
                Floor = facility.Floor,
                Equipment = facility.Equipment,
                Status = facility.Status,
                CreateAt = facility.CreateAt,
                UpdateAt = facility.UpdateAt,
                CampusName = facilityRequest.CampusName,
                TypeName = facilityRequest.TypeName
            };
        }
        public bool Delete(int id)
        {
            var facility = _facilityRepository.GetById(id);
            if (facility == null) return false;
            _facilityRepository.Delete(facility);
            return true;
        }
        public List<FacilityListResponse> GetFacilityList()
        {
            return _facilityRepository.GetFacilityList();
            
        }
    }
}
