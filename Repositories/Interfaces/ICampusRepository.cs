using SWP391_BL3.Models.Entities;

namespace SWP391_BL3.Repositories.Interfaces
{
    public interface ICampusRepository
    {
        IEnumerable<Campus> GetAll();
        Campus? GetByName(string campusName);
    }
}
