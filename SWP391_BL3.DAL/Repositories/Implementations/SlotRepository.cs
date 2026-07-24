using Microsoft.EntityFrameworkCore;
using SWP391_BL3.DAL.Data;
using SWP391_BL3.DAL.Models.Entities;
using SWP391_BL3.DAL.Repositories.Interfaces;
namespace SWP391_BL3.DAL.Repositories.Implementations
{
    public class SlotRepository : ISlotRepository
    {
        private readonly FptBookingContext _context;
        public SlotRepository(FptBookingContext context)
        {
            _context = context;
        }

        public IEnumerable<Slot> GetAll() => _context.Slots.ToList();
        public Slot? GetById(int id) => _context.Slots.Find(id);
        public Slot? GetByNumber(int slotNumber) => _context.Slots.FirstOrDefault(s => s.SlotNumber == slotNumber);
    }
}

