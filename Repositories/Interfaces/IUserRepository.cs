using SWP391_BL3.Models;
using SWP391_BL3.Models.Entities;
namespace SWP391_BL3.Repositories.Interfaces
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
    }
}
