using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391_BL3.Services.Interfaces;

namespace SWP391_BL3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacilitiTypeController : ControllerBase
    {
        private readonly IFacilityTypeService _facilityTypeService;
        public FacilitiTypeController(IFacilityTypeService facilityTypeService)
        {
            _facilityTypeService = facilityTypeService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var facilityTypes = _facilityTypeService.GetAll();
            return Ok(facilityTypes);
        }
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var facilityType = _facilityTypeService.GetById(id);
            if (facilityType == null)
            {
                return NotFound();
            }
            return Ok(facilityType);

        }
        [HttpPost]
        public IActionResult Create([FromBody] Models.DTOs.Request.FacilityTypeRequest facilityTypeRequest)
        {
            var createdFacilityType = _facilityTypeService.Create(facilityTypeRequest);
            return CreatedAtAction(nameof(Get), new { id = createdFacilityType.TypeId }, createdFacilityType);
        }
        [HttpPut]
        public IActionResult Update(int id, [FromBody] Models.DTOs.Request.FacilityTypeRequest facilityTypeRequest)
        {
            var updatedFacilityType = _facilityTypeService.Update(id, facilityTypeRequest);
            if (updatedFacilityType == null)
            {
                return NotFound();
            }
            return Ok(updatedFacilityType);
        }
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var isDeleted = _facilityTypeService.Delete(id);
            if (!isDeleted)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
