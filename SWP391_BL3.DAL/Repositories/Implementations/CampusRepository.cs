using Microsoft.EntityFrameworkCore;
using SWP391_BL3.DAL.Models.Entities;
using SWP391_BL3.DAL.Data;
using SWP391_BL3.DAL.Repositories.Interfaces;

namespace SWP391_BL3.DAL.Repositories.Implementations
{
    public class CampusRepository : ICampusRepository
    {
        private readonly FptBookingContext _context;
        public CampusRepository(FptBookingContext context)
        {
            _context = context;
        }
   
        public IEnumerable<Campus> GetAll()
        {
            return _context.Campuses.ToList();
        }
        public Campus? GetByName(string campusName)
        {
            return _context.Campuses
                           .FirstOrDefault(x => x.CampusName.ToLower() == campusName.ToLower());
        }
    }
}
