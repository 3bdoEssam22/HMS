using AutoMapper;
using HMS.Core.Entities.RoomModule;
using HMS.Shared.DataTransferObjects;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Services.Profiles
{
    internal class RoomImageResolver(IConfiguration _configuration) : IValueResolver<Room, RoomDetailsDto, List<string>>
    {
        public List<string> Resolve(Room source, RoomDetailsDto destination, List<string> destMember, ResolutionContext context)
        {
            return source.Images.Select(r => $"{_configuration["URLs:BaseURL"]}/images/rooms/{r.ImageUrl}").ToList();

        }
    }
}
