using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391_BL3.Models.DTOs.Request;
using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Services.Interfaces;


namespace SWP391_BL3.Controllers
{
    /*
     * ============================================
     * CONTROLLER - TRẢ LỜI CÂU HỎI HỘI ĐỒNG
     * ============================================
     * 
     * Q31: Có xử lý exception đầy đủ không?
     * A31: CÓ xử lý exception cơ bản:
     * - InvalidOperationException → 409 Conflict (business rule violation)
     * - ArgumentException → 400 BadRequest (invalid input)
     * - KeyNotFoundException → 404 NotFound (resource not found)
     * - Exception → 500 InternalServerError (unexpected error)
     * - Có thể cải thiện: Dùng global exception handler middleware để tránh code lặp lại
     * 
     * Q32: Response format có nhất quán không?
     * A32: CHƯA nhất quán.
     * - Success: Trả về object trực tiếp (Ok(result))
     * - Error: Trả về { message: "..." }
     * - Nên: Dùng ApiResponse wrapper để format nhất quán
     * - Ví dụ: { success: true, data: {...}, message: "..." }
     */
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }
        
        /*
         * Q33: Có validate input không?
         * A33: CÓ validate ở Service layer.
         * - Controller: Chỉ nhận request và gọi service
         * - Service: Validate business rules (date, capacity, conflict, etc.)
         * - Có thể cải thiện: Thêm Data Annotations hoặc FluentValidation ở DTO
         * - Ví dụ: [Required], [Range], [EmailAddress] attributes
         */
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
                // Q34: Có log exception không?
                // A34: CHƯA log. Nên log exception để debug và monitor
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
        [HttpPost("CheckIn/{bookingId}")]
        public IActionResult CheckIn(
            int bookingId,
            [FromBody] CheckInOutRequest request)
        {
            if (bookingId != request.BookingId)
                return BadRequest("BookingId không khớp");

            if (request.ImageUrls == null || !request.ImageUrls.Any())
                return BadRequest("Phải có ít nhất 1 hình ảnh");

            try
            {
                var result = _bookingService.CheckIn(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("CheckOut/{bookingId}")]
        public IActionResult CheckOut(
            int bookingId,
            [FromBody] CheckInOutRequest request)
        {
            if (bookingId != request.BookingId)
                return BadRequest("BookingId không khớp");

            if (request.ImageUrls == null || !request.ImageUrls.Any())
                return BadRequest("Phải có ít nhất 1 hình ảnh");

            try
            {
                var result = _bookingService.CheckOut(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("Cancel/{bookingId}")]
        public IActionResult Cancel(int bookingId)
        {
            var result = _bookingService.Cancel(bookingId);
            if (result == null) return NotFound("Booking not found");
            return Ok(result);
        }
    }
}

