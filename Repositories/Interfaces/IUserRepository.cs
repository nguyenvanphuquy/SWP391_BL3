using SWP391_BL3.Models;
using SWP391_BL3.Models.Entities;
namespace SWP391_BL3.Repositories.Interfaces
{
    public interface IUserRepository
    {
        User? GetbyEmail (string email);
    }
}
