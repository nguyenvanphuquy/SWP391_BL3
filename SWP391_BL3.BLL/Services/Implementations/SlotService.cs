using SWP391_BL3.DAL.Models.Entities;
using SWP391_BL3.DAL.Repositories.Interfaces;
using SWP391_BL3.BLL.Services.Interfaces;

namespace SWP391_BL3.BLL.Services.Implementations
{
    public class SlotService : ISlotService
    {
        private readonly ISlotRepository _slotRepository;

        public SlotService(ISlotRepository slotRepository)
        {
            _slotRepository = slotRepository;
        }

        public IEnumerable<Slot> GetAll() => _slotRepository.GetAll();
    }
}
