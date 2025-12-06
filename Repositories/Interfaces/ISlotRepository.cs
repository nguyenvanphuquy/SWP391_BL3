using SWP391_BL3.Models.Entities;

namespace SWP391_BL3.Repositories.Interfaces
{
    public interface ISlotRepository
    {
        IEnumerable<Slot> GetAll();
        Slot? GetById(int id);
        Slot? GetByNumber(int slotNumber);
    }
}
