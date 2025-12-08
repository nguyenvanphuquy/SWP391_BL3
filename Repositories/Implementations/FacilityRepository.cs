using Microsoft.Build.Construction;
using SWP391_BL3.Data;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Repositories.Interfaces;
using SWP391_BL3.Models.DTOs.Response;

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
        public Facility? GetByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            return _context.Facilities.FirstOrDefault(f => f.FacilityCode == code);
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
        public List<FacilityListResponse> GetFacilityList()
        {
            var list = (from f in _context.Facilities
                        join c in _context.Campuses on f.CampusId equals c.CampusId
                        join t in _context.FacilityTypes on f.TypeId equals t.TypeId
                        orderby f.CreateAt descending
                        select new FacilityListResponse
                        {
                            FacilityId = f.FacilityId,
                            FacilityCode = f.FacilityCode,
                            Capacity = f.Capacity,
                            Floors = f.Floor,
                            Equipment = f.Equipment,
                            Status = f.Status,
                            CampusName = c.CampusName,
                            TypeName = t.TypeName
                        }).ToList();
            return list;
        }
    }
}
