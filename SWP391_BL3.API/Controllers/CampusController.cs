using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391_BL3.BLL.Services.Interfaces;

namespace SWP391_BL3.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampusController : ControllerBase
    {
        private readonly ICampusService _campusService;
        public CampusController(ICampusService campusService)
        {
            _campusService = campusService;
        }
        [HttpGet]
        public IActionResult GetAll() 
            {
                var campus = _campusService.GetAll();
                return Ok(campus);
            }
    }
}
