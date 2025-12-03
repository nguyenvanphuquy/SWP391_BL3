using SWP391_BL3.Repositories.Interfaces;
using SWP391_BL3.Models;
using SWP391_BL3.Data;
using SWP391_BL3.Models.Entities;
namespace SWP391_BL3.Repositories.Implementations
{
    public class UserRepository: IUserRepository
    {
        private readonly FptBookingContext _context;
        public UserRepository(FptBookingContext context)
        {
            _context = context;
        }
        public User? GetbyEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }
    }
}
