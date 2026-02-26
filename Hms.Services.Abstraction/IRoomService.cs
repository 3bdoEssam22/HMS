using HMS.Shared.DataTransferObjects;
using HMS.Shared.QueryParameters;
using HMS.Shared.Respones;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hms.Services.Abstraction
{
    public interface IRoomService
    {
        Task<GenericResponse<IEnumerable<RoomDTO>>> GetAllRoomsForGuestAsync(
           string? roomType,
           string? sort
           );

        Task<GenericResponse<RoomDetailsDto>> GetRoomDetailsAsync(int roomId);

        Task<GenericResponse<IEnumerable<RoomAdminDTO>>> GetAllRoomsForAdminOrStaffAsync(RoomQueryParams? queryParams);

        Task<GenericResponse<bool>> CreateRoomAsync(CreateRoomDTO roomDTO);

        Task<GenericResponse<bool>> UpdateRoomAsync(int roomId, UpdateRoomDTO roomDTO);

        Task<GenericResponse<bool>> DeleteRoomAsync(int roomId);
        Task<GenericResponse<bool>> UploadImagesAsync(int roomId, List<IFormFile> files);

        Task<GenericResponse<bool>> DeleteRoomImageAsync(int roomId, int imageId);

    }
}
