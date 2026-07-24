using SWP391_BL3.DAL.Data;
using SWP391_BL3.DAL.Models.Entities;
using SWP391_BL3.DAL.Repositories.Interfaces;
namespace SWP391_BL3.DAL.Repositories.Implementations
{
    public class RoleRepository : IRoleRepository
    {
        private readonly FptBookingContext _context;
        public RoleRepository(FptBookingContext context)
        {
            _context = context;
        }
        public Role? GetById(int id)
        {
            return _context.Roles.FirstOrDefault(r => r.RoleId == id);
        }
        public Role? GetByName(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName)) return null;

            string normalizedName = roleName.Trim().ToLower();
            return _context.Roles
                .FirstOrDefault(r => r.RoleName.ToLower() == normalizedName);
        }
    }
}
