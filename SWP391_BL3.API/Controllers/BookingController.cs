using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391_BL3.DAL.Models.DTOs.Request;
using SWP391_BL3.DAL.Models.DTOs.Response;
using SWP391_BL3.BLL.Services.Interfaces;


namespace SWP391_BL3.API.Controllers
{
    /*
     * ============================================
     * CONTROLLER - TR? L?I CÂU H?I H?I Ð?NG
     * ============================================
     * 
     * Q31: Có x? lý exception d?y d? không?
     * A31: CÓ x? lý exception co b?n:
     * - InvalidOperationException ? 409 Conflict (business rule violation)
     * - ArgumentException ? 400 BadRequest (invalid input)
     * - KeyNotFoundException ? 404 NotFound (resource not found)
     * - Exception ? 500 InternalServerError (unexpected error)
     * - Có th? c?i thi?n: Dùng global exception handler middleware d? tránh code l?p l?i
     * 
     * Q32: Response format có nh?t quán không?
     * A32: CHUA nh?t quán.
     * - Success: Tr? v? object tr?c ti?p (Ok(result))
     * - Error: Tr? v? { message: "..." }
     * - Nên: Dùng ApiResponse wrapper d? format nh?t quán
     * - Ví d?: { success: true, data: {...}, message: "..." }
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
         * A33: CÓ validate ? Service layer.
         * - Controller: Ch? nh?n request và g?i service
         * - Service: Validate business rules (date, capacity, conflict, etc.)
         * - Có th? c?i thi?n: Thêm Data Annotations ho?c FluentValidation ? DTO
         * - Ví d?: [Required], [Range], [EmailAddress] attributes
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
                // Dùng cho conflict nhu: phòng dã b? d?t, s? lu?ng ngu?i vu?t capacity...
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Dùng cho d? li?u d?u vào sai: slot không t?n t?i, facility không t?n t?i, bookingDate sai
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // N?u service ném l?i khi không tìm th?y resource
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // L?i không xác d?nh
                // Q34: Có log exception không?
                // A34: CHUA log. Nên log exception d? debug và monitor
                return StatusCode(500, new { message = "L?i h? th?ng", detail = ex.Message });
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
                return BadRequest("BookingId không kh?p");

            if (request.ImageUrls == null || !request.ImageUrls.Any())
                return BadRequest("Ph?i có ít nh?t 1 hình ?nh");

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
                return BadRequest("BookingId không kh?p");

            if (request.ImageUrls == null || !request.ImageUrls.Any())
                return BadRequest("Ph?i có ít nh?t 1 hình ?nh");

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

