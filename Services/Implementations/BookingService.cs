using SWP391_BL3.Models.DTOs.Request;
using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Services.Interfaces;
using SWP391_BL3.Repositories.Interfaces;
namespace SWP391_BL3.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        public BookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }
        public BookingResponse CreateBooking(BookingRequest request)
        {
            var booking = new Booking
            {
                BookingCode = request.BookingCode,
                BookingDate = request.BookingDate,
                Purpose = request.Purpose,
                NumberOfMenber = request.NumberOfMember,
                UserId = request.UserId,
                FacilityId = request.FacilityId,
                Status = "Pending",
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            };

            var result = _bookingRepository.Create(booking);
            result = _bookingRepository.GetById(result.BookingId);

            return new BookingResponse
            {
                BookingId = result.BookingId,
                BookingCode = result.BookingCode,
                BookingDate = result.BookingDate,
                Purpose = result.Purpose,
                NumberOfMember = result.NumberOfMenber,
                Status = result.Status,
                UserFullName = result.User.FullName,
                FacilityCode = result.Facility.FacilityCode
            };
        }

        public BookingResponse UpdateBooking(int id, UpdateBookingRequest request)
        {
            var booking = _bookingRepository.GetById(id);
            if (booking == null) return null;

            booking.Purpose = request.Purpose;
            booking.NumberOfMenber = request.NumberOfMember;
            booking.Status = request.Status;
            booking.UpdateAt = DateTime.Now;

            _bookingRepository.Update(booking);

            booking = _bookingRepository.GetById(id);

            return new BookingResponse
            {
                BookingId = booking.BookingId,
                BookingCode = booking.BookingCode,
                BookingDate = booking.BookingDate,
                Purpose = booking.Purpose,
                NumberOfMember = booking.NumberOfMenber,
                Status = booking.Status,
                UserFullName = booking.User.FullName,
                FacilityCode = booking.Facility.FacilityCode
            };
        }

        public BookingResponse GetBooking(int id)
        {
            var booking = _bookingRepository.GetById(id);
            if (booking == null) return null;

            return new BookingResponse
            {
                BookingId = booking.BookingId,
                BookingCode = booking.BookingCode,
                BookingDate = booking.BookingDate,
                Purpose = booking.Purpose,
                NumberOfMember = booking.NumberOfMenber,
                Status = booking.Status,
                UserFullName = booking.User.FullName,
                FacilityCode = booking.Facility.FacilityCode
            };
        }

        public List<BookingResponse> GetAllBookings()
        {
            return _bookingRepository.GetAll()
                .Select(b => new BookingResponse
                {
                    BookingId = b.BookingId,
                    BookingCode = b.BookingCode,
                    BookingDate = b.BookingDate,
                    Purpose = b.Purpose,
                    NumberOfMember = b.NumberOfMenber,
                    Status = b.Status,
                    UserFullName = b.User.FullName,
                    FacilityCode = b.Facility.FacilityCode
                }).ToList();
        }

        public bool DeleteBooking(int id)
        {
            return _bookingRepository.Delete(id);
        }
        public List<BookingListResponse> GetBookingList()
        {
            return _bookingRepository.GetBookingList();
        }
    }
}