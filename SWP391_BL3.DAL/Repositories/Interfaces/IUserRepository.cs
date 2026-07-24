using SWP391_BL3.DAL.Models;
using SWP391_BL3.DAL.Models.Entities;
namespace SWP391_BL3.DAL.Repositories.Interfaces
{
    public interface IUserRepository
    {
        User? GetbyEmail (string email);
        User? GetById(int id);
        IEnumerable<User> GetAll();
        IEnumerable<User> GetAllInfor();
        void Create(User user);
        void Update(User user);
        void Delete(int id);
        void UpdateStatus(int id, string status);
        User? GetByGoogleId(string googleId);
    }
}
