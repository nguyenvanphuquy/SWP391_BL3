using Microsoft.EntityFrameworkCore;
using SWP391_BL3.Data;
using SWP391_BL3.Models;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Repositories.Interfaces;
namespace SWP391_BL3.Repositories.Implementations
{
    /*
     * Q24: Repository có SaveChanges() có đúng không?
     * A24: 
     * - Hiện tại: Repository tự gọi SaveChanges() → OK cho project nhỏ
     * - Vấn đề: Khó quản lý transaction cross-repository
     * - Nên: Repository chỉ thêm/sửa/xóa, Service gọi SaveChanges()
     * - Hoặc: Dùng UnitOfWork pattern để quản lý transaction tập trung
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
            // A25: KHÔNG. EF Core dùng parameterized query tự động
            // Email được truyền vào như parameter, không phải string concatenation
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }
        public User? GetById(int id) => _context.Users.Find(id);
        public IEnumerable<User> GetAll() => _context.Users.ToList();
        
        /*
         * Q26: GetAllInfor() có N+1 problem không?
         * A26: KHÔNG. Đã dùng Include() để eager load
         * - Include(u => u.Role): Load Role trong cùng query
         * - Include(u => u.BookingUsers): Load Bookings trong cùng query
         * - Tránh được N+1: 1 query thay vì 1 + N queries
         */
        public IEnumerable<User> GetAllInfor() => _context.Users.Include(u => u.Role).Include(u => u.BookingUsers).ToList();
        
        public void Create(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges(); // Repository tự save - OK cho project nhỏ
        }
        public void Update(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }
        
        /*
         * Q27: Có soft delete không?
         * A27: CÓ. Delete() thực chất là hard delete (xóa khỏi DB)
         * - UpdateStatus() mới là soft delete (set status = "Inactive")
         * - Nên: Đổi tên Delete() thành HardDelete(), và tạo SoftDelete()
         * - Hoặc: Chỉ dùng UpdateStatus() để soft delete
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
