using AutoMapper;
using HMS.Core.Entities.BookingModule;
using HMS.Shared.DataTransferObjects.BookingDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Services.Profiles
{
    public class BookingProfile : Profile
    {
        public BookingProfile()
        {
            CreateMap<Booking, BookingDTO>()
                .ForMember(dest => dest.GuestFullName, opt => opt.MapFrom(src => src.HotelUser.FullName))
                .ForMember(dest => dest.GuestEmail, opt => opt.MapFrom(src => src.HotelUser.Email));

            CreateMap<Booking, MyBookingsDTO>()
                .ForMember(dest => dest.CheckInDate, opt => opt.MapFrom(src => src.CheckInDate.ToShortDateString()))
                .ForMember(dest => dest.CheckOutDate, opt => opt.MapFrom(src => src.CheckOutDate.ToShortDateString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
