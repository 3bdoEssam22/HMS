using AutoMapper;
using Hms.Services.Abstraction;
using HMS.Core;
using HMS.Core.Contracts;
using HMS.Core.Entities.BookingModule;
using HMS.Core.Entities.RoomModule;
using HMS.Shared.DataTransferObjects.BookingDTOs;
using HMS.Shared.Messages;
using HMS.Shared.Respones;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Services.Services
{
    public class BookingService(IUnitOfWork _unitOfWork, ILogger<BookingService> _logger, IMapper _mapper, IEmailService _emailService) : IBookingService
    {

        public async Task<GenericResponse<Guid>> CreateBookingAsync(string userId, CreateBookingDTO createBooking)
        {
            var genericRespnse = new GenericResponse<Guid>();
            try
            {

                if (createBooking is null)
                {
                    genericRespnse.StatusCode = StatusCodes.Status400BadRequest;
                    genericRespnse.Message = "Enter Booking Data!";
                    return genericRespnse;
                }

                if (createBooking.CheckInDate < DateTime.Now || createBooking.CheckInDate >= createBooking.CheckOutDate)
                {
                    genericRespnse.StatusCode = StatusCodes.Status400BadRequest;
                    genericRespnse.Message = "Invalid Booking Data!";
                    return genericRespnse;
                }

                var room = await _unitOfWork.GetRepository<Room, int>().GetByIdAsync(createBooking.RoomId, null, [r => r.RoomBookings]);

                if (room is null || room.RoomStatus == RoomStatus.Maintenance || room.RoomStatus == RoomStatus.NotExist)
                {
                    genericRespnse.StatusCode = StatusCodes.Status404NotFound;
                    genericRespnse.Message = "Room is not found!";
                    return genericRespnse;
                }

                var hasConflict = room.RoomBookings
                    .Any(b => (b.Status == BookingStatus.PendingPayment || b.Status == BookingStatus.Paid) &&
                        (b.CheckInDate < createBooking.CheckOutDate) && (b.CheckOutDate > createBooking.CheckInDate));
                if (hasConflict)
                {
                    genericRespnse.StatusCode = StatusCodes.Status400BadRequest;
                    genericRespnse.Message = "Room is not Available!";
                    return genericRespnse;
                }

                var nights = (createBooking.CheckOutDate - createBooking.CheckInDate).Days;
                var totalAmount = room.PricePerNight * nights;

                var booking = new Booking()
                {
                    Id = Guid.NewGuid(),
                    CheckInDate = createBooking.CheckInDate,
                    CheckOutDate = createBooking.CheckOutDate,
                    HotelUserId = userId,
                    RoomId = createBooking.RoomId,
                    TotalAmount = totalAmount
                };

                await _unitOfWork.GetRepository<Booking, Guid>().AddAsync(booking);
                var result = await _unitOfWork.SaveChangesAsync() > 0;

                if (result)
                {
                    genericRespnse.StatusCode = StatusCodes.Status200OK;
                    genericRespnse.Message = "Booking Created Successfully!";
                    genericRespnse.Data = booking.Id;
                }
                else
                {
                    genericRespnse.StatusCode = StatusCodes.Status500InternalServerError;
                    genericRespnse.Message = "Booking is not created!";
                }
                return genericRespnse;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot Create Booking");
                genericRespnse.StatusCode = StatusCodes.Status500InternalServerError;
                genericRespnse.Message = "Booking is not created!";
                return genericRespnse;
            }

        }

        public async Task<GenericResponse<IEnumerable<BookingDTO>>> GetAllBookingForAdminAsync()
        {
            var genericResponse = new GenericResponse<IEnumerable<BookingDTO>>();
            var bookings = await _unitOfWork.GetRepository<Booking, Guid>().GetAllAsync(null, null, x => x.CreatedAt, [b => b.HotelUser]);

            if (bookings is null || !bookings.Any())
            {
                genericResponse.StatusCode = StatusCodes.Status404NotFound;
                genericResponse.Message = "No Bookings Found!";
                return genericResponse;
            }

            var mappedBookings = _mapper.Map<IEnumerable<Booking>, IEnumerable<BookingDTO>>(bookings);

            genericResponse.StatusCode = StatusCodes.Status200OK;
            genericResponse.Message = "Bookings Retrieved Successfully!";
            genericResponse.Data = mappedBookings;
            return genericResponse;

        }
        public async Task<GenericResponse<bool>> CancelBookingAsync(Guid bookingId)
        {
            var genericResponse = new GenericResponse<bool>();
            try
            {

                var booking = await _unitOfWork.GetRepository<Booking, Guid>().GetByIdAsync(bookingId, null, [b => b.HotelUser]);

                if (booking is null)
                {
                    genericResponse.StatusCode = StatusCodes.Status404NotFound;
                    genericResponse.Message = "Booking is not found!";
                    return genericResponse;
                }

                if (booking.Status == BookingStatus.Paid)
                {
                    genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                    genericResponse.Message = "Cannot cancel a paid booking!";
                    return genericResponse;
                }

                booking.Status = BookingStatus.Canceled;
                booking.UpdatedAt = DateTime.Now;

                _unitOfWork.GetRepository<Booking, Guid>().Update(booking);

                var result = await _unitOfWork.SaveChangesAsync() > 0;
                if (result)
                {
                    genericResponse.StatusCode = StatusCodes.Status200OK;
                    genericResponse.Message = "Booking Canceled Successfully!";
                    genericResponse.Data = true;
                    var email = new Email()
                    {
                        To = booking.HotelUser.Email!,
                        Subject = "Booking Canceled",
                        Body = "Your booking has been canceled."
                    };
                    await _emailService.SendEmailAsync(email);
                }
                else
                {
                    genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                    genericResponse.Message = "Booking is not canceled!";
                    genericResponse.Data = false;
                }
                return genericResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot Cancel Booking");
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Message = "Booking is not canceled!";
                return genericResponse;
            }
        }

        public async Task<GenericResponse<IEnumerable<MyBookingsDTO>>> GetAllBookingsForGuest(string GuestId)
        {
            var genericResponse = new GenericResponse<IEnumerable<MyBookingsDTO>>();

            var bookings = await _unitOfWork.GetRepository<Booking, Guid>().GetAllAsync(b => b.HotelUserId == GuestId, null, x => x.CheckInDate);

            if (bookings is null || !bookings.Any())
            {
                genericResponse.StatusCode = StatusCodes.Status404NotFound;
                genericResponse.Message = "No Bookings Found!";
                return genericResponse;
            }
            var mappedBookings = _mapper.Map<IEnumerable<Booking>, IEnumerable<MyBookingsDTO>>(bookings);
            genericResponse.StatusCode = StatusCodes.Status200OK;
            genericResponse.Message = "Bookings Retrieved Successfully!";
            genericResponse.Data = mappedBookings;
            return genericResponse;
        }

    }
}
