using AutoMapper;
using HMS.Core.Entities.RoomModule;
using HMS.Shared.DataTransferObjects.RoomDTOs;

namespace HMS.Services.Profiles
{
    internal class RoomProfile : Profile
    {
        public RoomProfile()
        {
            CreateMap<Room, RoomDTO>();
            CreateMap<Room, RoomDetailsDto>()
                .ForMember(dest => dest.ImagesUrl, opt => opt.MapFrom<RoomImageResolver>());
            CreateMap<Room, RoomAdminDTO>();

            CreateMap<CreateRoomDTO, Room>();
            CreateMap<UpdateRoomDTO, Room>();

        }
    }
}
