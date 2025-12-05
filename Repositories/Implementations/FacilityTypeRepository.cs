using SWP391_BL3.Data;
using SWP391_BL3.Repositories.Interfaces;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Models.DTOs.Response;
namespace SWP391_BL3.Repositories.Implementations
{
    public class FacilityTypeRepository : IFacilityTypeRepository
    {
        private readonly FptBookingContext _context;
        public FacilityTypeRepository(FptBookingContext context)
        {
            _context = context;
        }
        public IEnumerable<FacilityType> GetAll()
        {
            return _context.FacilityTypes.ToList();
        }
        public FacilityType? GetById(int id)
        {
            return _context.FacilityTypes.Find(id);
        }
        public void Create(FacilityType facilityType)
        {
            _context.FacilityTypes.Add(facilityType);
            _context.SaveChanges();
        }
        public void Update(FacilityType facilityType)
        {
            _context.FacilityTypes.Update(facilityType);
            _context.SaveChanges();
        }
        public void Delete(FacilityType facilityType)
        {
            _context.FacilityTypes.Remove(facilityType);
            _context.SaveChanges();
        }
        public List<ListTypeResponse> GetListType()
        {
            var data = (from ft in _context.FacilityTypes
                        orderby ft.CreateAt descending
                        select new  ListTypeResponse
                        {
                            TypeId = ft.TypeId,
                            TypeName = ft.TypeName,
                            TypeDescription = ft.Description,
                            FacilitiCount = _context.Facilities.Count(i => i.TypeId == ft.TypeId),
                            CreateAt = ft.CreateAt,
                        })
                        .ToList();
            return data;
        }

    }
}

