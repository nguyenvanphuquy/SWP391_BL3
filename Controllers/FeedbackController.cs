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
            try
            {
                var feedback = _feedbackService.Create(req);
                return Ok(new
                {
                    success = true,
                    message = "Feedback thành công!",
                    data = feedback
                });
            }
            catch (InvalidOperationException ex)
            {
                // Trả về message thân thiện hơn
                return BadRequest(new
                {
                    success = false,
                    message = "Bạn đã đánh giá phòng này rồi. Mỗi người chỉ được đánh giá 1 lần.",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                // Log error nếu cần
                // _logger.LogError(ex, "Error creating feedback");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau."
                });
            }
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
        [HttpGet("feedbacklist")]
        public IActionResult GetFeedbackList()
        {
            var feedbackList = _feedbackService.GetFeedbackList();
            return Ok(feedbackList);
        }
        [HttpGet("feedbackdetail/{feedbackId}")]
        public IActionResult GetFeedbackDetail(int feedbackId)
        {
            var feedbackDetail = _feedbackService.GetFeedbackDetail(feedbackId);
            if (feedbackDetail == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Feedback not found."
                });
            }
            return Ok(new
            {
                success = true,
                data = feedbackDetail
            });
        }
    }
}

