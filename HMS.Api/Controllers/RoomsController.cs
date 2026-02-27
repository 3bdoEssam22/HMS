using Hms.Services.Abstraction;
using HMS.Shared.DataTransferObjects;
using HMS.Shared.QueryParameters;
using HMS.Shared.Respones;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Api.Controllers
{
    public class RoomsController(IRoomService _roomService) : BaseApiController
    {
        #region Guest Controllers

        //GET baseUrl/api/rooms/public
        [HttpGet("public")]
        public async Task<ActionResult<GenericResponse<IEnumerable<RoomDTO>>>> GetAllRoomsForGuest(string? roomType, string? sort)
        {
            var rooms = await _roomService.GetAllRoomsForGuestAsync(roomType, sort);
            return HandleRespone(rooms);
        }

        // Get baseUrl/api/rooms/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<GenericResponse<RoomDetailsDto>>> GetRoomByIdGuestAsync(int id)
        {
            var room = await _roomService.GetRoomDetailsAsync(id);
            return HandleRespone(room);
        }


        #endregion

        #region Admin Controllers

        // Get BaseUrl/api/Rooms/admin
        [HttpGet("admin")]
        public async Task<ActionResult<GenericResponse<IEnumerable<RoomAdminDTO>>>> GetRoomsForAdminAndStaff([FromQuery] RoomQueryParams? queryParams)
        {
            var rooms = await _roomService.GetAllRoomsForAdminOrStaffAsync(queryParams);
            return HandleRespone(rooms);
        }

        //Post BaseUrl/api/Rooms
        [HttpPost]
        public async Task<ActionResult<GenericResponse<bool>>> CreateRoom(CreateRoomDTO roomDTO)
        {
            var room = await _roomService.CreateRoomAsync(roomDTO);
            return HandleRespone(room);
        }

        //Put BaseUrl/api/Rooms/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<GenericResponse<bool>>> UpdateRoom([FromRoute] int id, [FromBody] UpdateRoomDTO roomDTO)
        {
            var result = await _roomService.UpdateRoomAsync(id, roomDTO);
            return HandleRespone(result);
        }

        //Delete BaseUrl/api/Rooms/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<GenericResponse<bool>>> DeleteRoom(int id)
        {
            var result = await _roomService.DeleteRoomAsync(id);
            return HandleRespone(result);
        }

        //Post BaseUrl/api/Rooms/{id}/images
        [HttpPost("{id}/images")]
        public async Task<ActionResult<GenericResponse<bool>>> AddImages([FromRoute] int id, [FromForm] List<IFormFile> files)
        {
            var result = await _roomService.UploadImagesAsync(id, files);
            return HandleRespone(result);
        }

        //Delete BaseUrl/api/Rooms/{id}/images/{imageId}
        [HttpDelete("{id}/images/{imageId}")]
        public async Task<ActionResult<GenericResponse<bool>>> DeleteImage(int id, int imageId)
        {
            var result = await _roomService.DeleteRoomImageAsync(id, imageId);
            return HandleRespone(result);
        }
        #endregion

    }
}
