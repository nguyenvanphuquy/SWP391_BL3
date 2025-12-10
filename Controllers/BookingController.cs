using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391_BL3.Models.DTOs.Request;
using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Services.Interfaces;


namespace SWP391_BL3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }
        [HttpPost]
        public IActionResult Create(BookingRequest request)
        {
            try
            {
                var result = _bookingService.CreateBooking(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                // Dùng cho conflict như: phòng đã bị đặt, số lượng người vượt capacity...
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Dùng cho dữ liệu đầu vào sai: slot không tồn tại, facility không tồn tại, bookingDate sai
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // Nếu service ném lỗi khi không tìm thấy resource
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Lỗi không xác định
                return StatusCode(500, new { message = "Lỗi hệ thống", detail = ex.Message });
            }
        }


        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateBookingRequest request, int currentUserId)
        {
            var result = _bookingService.UpdateBooking(id, request, currentUserId);
            if (result == null) return NotFound("Booking not found");
            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var result = _bookingService.GetBooking(id);
            if (result == null) return NotFound("Booking not found");
            return Ok(result);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_bookingService.GetAllBookings());
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!_bookingService.DeleteBooking(id))
                return NotFound("Booking not found");

            return Ok("Deleted successfully");
        }
        [HttpGet("List")]
        public IActionResult GetBookingList()
        {
            var result = _bookingService.GetBookingList();
            return Ok(result);
        }
        [HttpGet("Detail/{bookingId}")]
        public IActionResult GetBookingDetail(int bookingId)
        {
            var result = _bookingService.GetBookingDetail(bookingId);
            if (result == null) return NotFound("Booking not found");
            return Ok(result);
        }
        [HttpGet("User/{userId}")]
        public IActionResult GetBookingsByUser(int userId)
        {
            var result = _bookingService.GetListBookingUsers(userId);
            return Ok(result);
        }
        [HttpGet("Stats/{userId}")]
        public IActionResult GetUserBookingStats(int userId)
        {
            var result = _bookingService.GetUserBookingStats(userId);
            return Ok(result);
        }
    }
}

