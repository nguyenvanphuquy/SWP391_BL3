using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391_BL3.Models.DTOs.Request;
using SWP391_BL3.Services.Interfaces;

namespace SWP391_BL3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;
        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }
        [HttpPost]
        public IActionResult Create(FeedbackRequest req)
        {
            return Ok(_feedbackService.Create(req));
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateFeedbackRequest req)
        {
            var result = _feedbackService.Update(id, req);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!_feedbackService.Delete(id)) return NotFound();
            return Ok("Deleted");
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var fb = _feedbackService.GetById(id);
            if (fb == null) return NotFound();
            return Ok(fb);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_feedbackService.GetAll());
        }

        [HttpGet("facility/{facilityId}")]
        public IActionResult GetByFacility(int facilityId)
        {
            return Ok(_feedbackService.GetByFacility(facilityId));
        }
    }
}

