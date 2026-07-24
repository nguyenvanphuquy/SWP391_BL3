using Microsoft.AspNetCore.Mvc;
using SWP391_BL3.BLL.Services.Interfaces;

namespace SWP391_BL3.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SlotController : ControllerBase
    {
        private readonly ISlotService _slotService;
        public SlotController(ISlotService slotService)
        {
            _slotService = slotService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var slots = _slotService.GetAll();
            return Ok(slots);
        }
    }
}
