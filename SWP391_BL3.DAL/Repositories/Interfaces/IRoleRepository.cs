using SWP391_BL3.DAL.Models.Entities;

namespace SWP391_BL3.DAL.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Role? GetById(int id);
        Role? GetByName(string roleName);
    }
}
