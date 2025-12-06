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
            var result = _bookingService.CreateBooking(request);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateBookingRequest request)
        {
            var result = _bookingService.UpdateBooking(id, request);
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
    }
}

