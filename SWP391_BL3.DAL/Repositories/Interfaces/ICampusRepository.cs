using SWP391_BL3.DAL.Models.Entities;

namespace SWP391_BL3.DAL.Repositories.Interfaces
{
    public interface ICampusRepository
    {
        IEnumerable<Campus> GetAll();
        Campus? GetByName(string campusName);
    }
}
