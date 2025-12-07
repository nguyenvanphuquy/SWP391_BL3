using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391_BL3.Repositories.Interfaces;

namespace SWP391_BL3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SlotController : ControllerBase
    {
        private readonly ISlotRepository _slotRepository;
        public SlotController(ISlotRepository slotRepository)
        {
            _slotRepository = slotRepository;
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            var slots = _slotRepository.GetAll();
            return Ok(slots);
        }
    }
}
