using SWP391_BL3.Data;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Repositories.Interfaces;

namespace SWP391_BL3.Repositories.Implementations
{
    public class FacilityRepository : IFacilityRepository
    {
        private readonly FptBookingContext _context;
        public FacilityRepository(FptBookingContext context)
        {
            _context = context;
        }
        public IEnumerable<Facility> GetAll()
        {
            return _context.Facilities.ToList();
        }
        public Facility? GetById(int id)
        {
            return _context.Facilities.Find(id);
        }
        public void Create(Facility facility)
        {
            _context.Facilities.Add(facility);
            _context.SaveChanges();
        }
        public void Update(Facility facility)
        {
            _context.Facilities.Update(facility);
            _context.SaveChanges();
        }
        public void Delete(Facility facility)
        {
            _context.Facilities.Remove(facility);
            _context.SaveChanges();
        }
    }
}
