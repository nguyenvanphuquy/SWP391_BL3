using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391_BL3.Models.DTOs.Request;
using SWP391_BL3.Services.Implementations;
using SWP391_BL3.Services.Interfaces;

namespace SWP391_BL3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacilitiController : ControllerBase
    {
        private readonly IFacilityService _facilityService;
        public FacilitiController(IFacilityService facilityService)
        {
            _facilityService = facilityService;
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            var facilityTypes = _facilityService.GetAll();
            return Ok(facilityTypes);
        }
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var facilityType = _facilityService.GetById(id);
            if (facilityType == null)
            {
                return NotFound();
            }
            return Ok(facilityType);

        }
        [HttpPost("create")]
        public IActionResult CreateFacility([FromBody] FacilityRequest facilityRequest)
        {
            var result = _facilityService.CreateFacility(facilityRequest);
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public IActionResult UpdateFacility(int id, [FromBody] FacilityRequest facilityRequest)
        {
            var result = _facilityService.UpdateFacility(id, facilityRequest);
            return Ok(result);
        }
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var isDeleted = _facilityService.Delete(id);
            if (!isDeleted)
            {
                return NotFound();
            }
            return NoContent();
        }
        [HttpGet("List")]
        public IActionResult GetFacilityList()
        {
            var result = _facilityService.GetFacilityList();
            return Ok(result);
        }
        [HttpGet("Detail/{facilityId}")]
        public IActionResult GetFacilityDetail(int facilityId)
        {
            var result = _facilityService.GetFacilityDetail(facilityId);
            return Ok(result);
        }
    }

}
