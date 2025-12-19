using SWP391_BL3.Models.Entities;

namespace SWP391_BL3.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Role? GetById(int id);
        Role? GetByName(string roleName);
    }
}
