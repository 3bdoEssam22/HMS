using HMS.Shared.DataTransferObjects.BookingDTOs;
using HMS.Shared.Respones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hms.Services.Abstraction
{
    public interface IBookingService
    {
        Task<GenericResponse<Guid>> CreateBookingAsync(string userId, CreateBookingDTO createBooking);
        Task<GenericResponse<IEnumerable<BookingDTO>>> GetAllBookingForAdminAsync();
        Task<GenericResponse<bool>> CancelBookingAsync(Guid bookingId);
        Task<GenericResponse<IEnumerable<MyBookingsDTO>>> GetAllBookingsForGuest(string GuestId);
    }
}
