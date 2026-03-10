using Hms.Services.Abstraction;
using HMS.Shared.DataTransferObjects.BookingDTOs;
using HMS.Shared.Respones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HMS.Api.Controllers
{
    public class BookingsController(IBookingService _bookingService, IPaymentService _PaymentService)
        : BaseApiController
    {

        //Post BaseUrl/api/Bookings
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<GenericResponse<Guid>>> CreateBooking(CreateBookingDTO createBooking)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _bookingService.CreateBookingAsync(userId!, createBooking);
            return HandleRespone(result);
        }

        [Authorize(Roles = "Guest")]
        [HttpPost("{id}/pay")]
        public async Task<ActionResult<GenericResponse<string>>> CreatePaymentUrl(Guid id)
        {
            var result = await _PaymentService.CreatePaymentUrlAsync(id);
            return HandleRespone(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        //Get BaseUrl/api/Bookings/admin
        public async Task<ActionResult<GenericResponse<IEnumerable<BookingDTO>>>> GetAllBookingsForAdmin()
        {
            var result = await _bookingService.GetAllBookingForAdminAsync();
            return HandleRespone(result);
        }

        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GenericResponse<bool>>> CancelBooking(Guid id)
        {
            var result = await _bookingService.CancelBookingAsync(id);
            return HandleRespone(result);
        }

        [Authorize(Roles = "Guest")]
        [HttpGet("{my}")]
        public async Task<ActionResult<GenericResponse<IEnumerable<MyBookingsDTO>>>> GetAllBookingsForGuest()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _bookingService.GetAllBookingsForGuest(userId!);
            return HandleRespone(result);
        }
    }
}
