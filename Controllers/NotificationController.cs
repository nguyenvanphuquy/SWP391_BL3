using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391_BL3.Services.Interfaces;

namespace SWP391_BL3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
    [HttpGet("user/{userId}")]
        public IActionResult GetUserNotifications(int userId)
        {
            try
            {
                var notifications = _notificationService.GetUserNotifications(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Notification/user/{userId}/unread
        [HttpGet("user/{userId}/unread")]
        public IActionResult GetUnreadNotifications(int userId)
        {
            try
            {
                var notifications = _notificationService.GetUnreadNotifications(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Notification/user/{userId}/unread-count
        [HttpGet("user/{userId}/unread-count")]
        public IActionResult GetUnreadCount(int userId)
        {
            try
            {
                var count = _notificationService.GetUnreadCount(userId);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Notification/{id}
        [HttpGet("{id}")]
        public IActionResult GetNotificationById(int id)
        {
            try
            {
                var notification = _notificationService.GetNotificationById(id);
                if (notification == null)
                {
                    return NotFound(new { message = "Notification không tồn tại" });
                }
                return Ok(notification);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Notification/{id}/mark-read
        [HttpPut("{id}/mark-read")]
        public IActionResult MarkAsRead(int id)
        {
            try
            {
                var result = _notificationService.MarkAsRead(id);
                if (!result)
                {
                    return NotFound(new { message = "Notification không tồn tại" });
                }
                return Ok(new { message = "Đã đánh dấu là đã đọc" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Notification/user/{userId}/mark-all-read
        [HttpPut("user/{userId}/mark-all-read")]
        public IActionResult MarkAllAsRead(int userId)
        {
            try
            {
                var result = _notificationService.MarkAllAsRead(userId);
                if (!result)
                {
                    return NotFound(new { message = "Không có notification nào để đánh dấu" });
                }
                return Ok(new { message = "Đã đánh dấu tất cả là đã đọc" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/Notification/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteNotification(int id)
        {
            try
            {
                var result = _notificationService.DeleteNotification(id);
                if (!result)
                {
                    return NotFound(new { message = "Notification không tồn tại" });
                }
                return Ok(new { message = "Đã xóa notification" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
