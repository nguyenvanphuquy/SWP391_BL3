using Microsoft.EntityFrameworkCore;
using SWP391_BL3.Data;
using SWP391_BL3.Models;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Repositories.Interfaces;
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
        public User? GetById(int id) => _context.Users.Find(id);
        public IEnumerable<User> GetAll() => _context.Users.ToList();
        public IEnumerable<User> GetAllInfor() => _context.Users.Include(u => u.Role).Include(u => u.BookingUsers).ToList();
        public void Create(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }
        public void Update(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }
        public void UpdateStatus(int id, string status)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                user.Status = status;
                _context.SaveChanges();
            }
        }
    }
}
