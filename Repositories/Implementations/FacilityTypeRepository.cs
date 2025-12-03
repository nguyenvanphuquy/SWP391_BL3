using SWP391_BL3.Data;
using SWP391_BL3.Repositories.Interfaces;
using SWP391_BL3.Models.Entities;
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
    }
}

