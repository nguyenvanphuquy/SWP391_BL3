using Microsoft.EntityFrameworkCore;
using SWP391_BL3.DAL.Data;
using SWP391_BL3.DAL.Models;
using SWP391_BL3.DAL.Models.Entities;
using SWP391_BL3.DAL.Repositories.Interfaces;
namespace SWP391_BL3.DAL.Repositories.Implementations
{
    /*
     * Q24: Repository có SaveChanges() có dúng không?
     * A24: 
     * - Hi?n t?i: Repository t? g?i SaveChanges() ? OK cho project nh?
     * - V?n d?: Khó qu?n lý transaction cross-repository
     * - Nên: Repository ch? thêm/s?a/xóa, Service g?i SaveChanges()
     * - Ho?c: Dùng UnitOfWork pattern d? qu?n lý transaction t?p trung
     */
    public class UserRepository: IUserRepository
    {
        private readonly FptBookingContext _context;
        public UserRepository(FptBookingContext context)
        {
            _context = context;
        }
        public User? GetbyEmail(string email)
        {
            // Q25: Có SQL Injection risk không?
            // A25: KHÔNG. EF Core dùng parameterized query t? d?ng
            // Email du?c truy?n vào nhu parameter, không ph?i string concatenation
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }
        public User? GetById(int id) => _context.Users.Find(id);
        public IEnumerable<User> GetAll() => _context.Users.ToList();
        
        /*
         * Q26: GetAllInfor() có N+1 problem không?
         * A26: KHÔNG. Ðã dùng Include() d? eager load
         * - Include(u => u.Role): Load Role trong cùng query
         * - Include(u => u.BookingUsers): Load Bookings trong cùng query
         * - Tránh du?c N+1: 1 query thay vì 1 + N queries
         */
        public IEnumerable<User> GetAllInfor() => _context.Users.Include(u => u.Role).Include(u => u.BookingUsers).ToList();
        
        public void Create(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges(); // Repository t? save - OK cho project nh?
        }
        public void Update(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }
        
        /*
         * Q27: Có soft delete không?
         * A27: CÓ. Delete() th?c ch?t là hard delete (xóa kh?i DB)
         * - UpdateStatus() m?i là soft delete (set status = "Inactive")
         * - Nên: Ð?i tên Delete() thành HardDelete(), và t?o SoftDelete()
         * - Ho?c: Ch? dùng UpdateStatus() d? soft delete
         */
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
        public User? GetByGoogleId(string googleId)
        {
            return _context.Users.FirstOrDefault(u => u.GoogleId == googleId);
        }
    }
}
