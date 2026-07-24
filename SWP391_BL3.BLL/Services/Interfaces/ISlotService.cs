using SWP391_BL3.DAL.Models.Entities;
using SWP391_BL3.DAL.Repositories.Interfaces;

namespace SWP391_BL3.BLL.Services.Interfaces
{
    public interface ISlotService
    {
        IEnumerable<Slot> GetAll();
    }
}
