using AutoMapper;
using Hms.Services.Abstraction;
using HMS.Core;
using HMS.Core.Contracts;
using HMS.Core.Entities.RoomModule;
using HMS.Shared.DataTransferObjects.RoomDTOs;
using HMS.Shared.QueryParameters;
using HMS.Shared.Respones;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace HMS.Services.Services
{
    public class RoomService(IUnitOfWork _unitOfWork, IMapper _mapper, ILogger<RoomService> _logger, IAttachmentService _attachmentService) : IRoomService
    {
        public async Task<GenericResponse<bool>> CreateRoomAsync(CreateRoomDTO roomDTO)
        {
            var genericResponse = new GenericResponse<bool>();
            try
            {
                if (roomDTO is null)
                {
                    genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                    genericResponse.Message = "Invalid Room Data.";

                    return genericResponse;
                }

                var createdroom = _mapper.Map<Room>(roomDTO);
                createdroom.RoomStatus = RoomStatus.Available;

                await _unitOfWork.GetRepository<Room, int>().AddAsync(createdroom);
                var result = await _unitOfWork.SaveChangesAsync() > 0;

                if (result)
                {
                    genericResponse.StatusCode = StatusCodes.Status200OK;
                    genericResponse.Message = "Room Created Succefully";
                    genericResponse.Data = true;
                }
                else
                {
                    genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                    genericResponse.Message = "Room is not created!";
                    genericResponse.Data = false;
                }
                return genericResponse;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a room.");
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Message = "Room is not created!";
                genericResponse.Data = false;
                return genericResponse;
            }
        }

        public async Task<GenericResponse<bool>> DeleteRoomImageAsync(int roomId, int imageId)
        {
            var genericResponse = new GenericResponse<bool>();

            try
            {
                var room = await _unitOfWork.GetRepository<Room, int>().GetByIdAsync(roomId, null, [r => r.Images]);

                if (room is null)
                {
                    genericResponse.StatusCode = StatusCodes.Status404NotFound;
                    genericResponse.Message = "Room is not found!";
                    return genericResponse;
                }
                if (room.Images is null || room.Images.Count == 0)
                {
                    genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                    genericResponse.Message = "There is no images for this room!";
                    return genericResponse;
                }

                var roomImage = room.Images.FirstOrDefault(r => r.Id == imageId);

                if (roomImage is null)
                {
                    genericResponse.StatusCode = StatusCodes.Status404NotFound;
                    genericResponse.Message = "Image is not found";
                    return genericResponse;
                }

                _unitOfWork.GetRepository<RoomImage, int>().Delete(roomImage); //Deleted

                var isDeletedFromServer = _attachmentService.DeleteFile(roomImage.ImageUrl, "rooms");

                if (!isDeletedFromServer)
                {
                    genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                    genericResponse.Message = "Failed to delete the image!";
                    return genericResponse;
                }

                var result = await _unitOfWork.SaveChangesAsync() > 0;
                if (result)
                {
                    genericResponse.StatusCode = StatusCodes.Status200OK;
                    genericResponse.Message = "Image is deleted successfully.";
                    genericResponse.Data = true;
                }
                else
                {
                    genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                    genericResponse.Message = "Failed to delete the image!";
                }
                return genericResponse;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete the image!");
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Message = "Failed to delete the image!";
                return genericResponse;
            }
        }

        public async Task<GenericResponse<bool>> DeleteRoomAsync(int roomId)
        {
            var genericResponse = new GenericResponse<bool>();

            try
            {
                var roomRepo = _unitOfWork.GetRepository<Room, int>();
                // NOT Completed Logic
                var room = await roomRepo
                    .GetByIdAsync(roomId); //TODO Check Future Booking

                if (room is null)
                {
                    genericResponse.StatusCode = StatusCodes.Status404NotFound;
                    genericResponse.Message = "Room is not found.";
                    return genericResponse;
                }

                room.RoomStatus = RoomStatus.NotExist;
                roomRepo.Update(room);

                var result = await _unitOfWork.SaveChangesAsync() > 0;

                if (result)
                {
                    genericResponse.StatusCode = StatusCodes.Status200OK;
                    genericResponse.Message = "Room is deleted successfully.";
                    genericResponse.Data = true;
                }
                else
                {
                    genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                    genericResponse.Message = "Room is not deleted.";
                }
                return genericResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while deleting the room");
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Message = "Room is not deleted.";
                return genericResponse;
            }

        }

        public async Task<GenericResponse<IEnumerable<RoomAdminDTO>>> GetAllRoomsForAdminOrStaffAsync(RoomQueryParams? queryParams)
        {
            var genericResponse = new GenericResponse<IEnumerable<RoomAdminDTO>>();
            IEnumerable<Room>? rooms = null;

            if (queryParams is not null)
            {
                Enum.TryParse(queryParams.roomType, out RoomType roomTypeEnum);
                Enum.TryParse(queryParams.roomStatus, out RoomStatus roomStatusEnum);

                Expression<Func<Room, bool>> filter = r => (queryParams.roomType == null || r.RoomType == roomTypeEnum) && (queryParams.roomStatus == null || r.RoomStatus == roomStatusEnum);

                Expression<Func<Room, object>>? orderByExp = null;
                Expression<Func<Room, object>>? orderByDescExp = null;
                if (queryParams.sort is not null)
                {
                    switch (queryParams.sort)
                    {
                        case "PriceAsc":
                            orderByExp = r => r.PricePerNight;
                            break;
                        case "PriceDesc":
                            orderByDescExp = r => r.PricePerNight;
                            break;
                        default:
                            orderByExp = r => r.CreatedAt;
                            break;
                    }
                }
                else
                    orderByExp = r => r.CreatedAt;

                rooms = await _unitOfWork.GetRepository<Room, int>().GetAllAsync(filter, orderByExp, orderByDescExp);
            }
            else
                rooms = await _unitOfWork.GetRepository<Room, int>().GetAllAsync();

            if (rooms is null || !rooms.Any())
            {
                genericResponse.StatusCode = StatusCodes.Status404NotFound;
                genericResponse.Message = "No Rooms Found";
                return genericResponse;
            }

            var roomsDto = _mapper.Map<IEnumerable<RoomAdminDTO>>(rooms);
            genericResponse.StatusCode = StatusCodes.Status200OK;
            genericResponse.Message = "Rooms Retrieved Successfully";
            genericResponse.Data = roomsDto;
            return genericResponse;
        }

        public async Task<GenericResponse<IEnumerable<RoomDTO>>> GetAllRoomsForGuestAsync(string? roomType, string? sort)
        {
            var genericResponse = new GenericResponse<IEnumerable<RoomDTO>>();

            Enum.TryParse(roomType, out RoomType roomTypeEnum);
            Expression<Func<Room, bool>> filter = r => (roomType == null || r.RoomType == roomTypeEnum) && (r.RoomStatus == RoomStatus.Available || r.RoomStatus == RoomStatus.Reserved);

            Expression<Func<Room, object>>? orderBy = null;
            Expression<Func<Room, object>>? orderByDesc = null;

            if (sort is not null)
            {
                switch (sort)
                {
                    case "PriceAsc":
                        orderBy = R => R.PricePerNight;
                        break;

                    case "PriceDesc":
                        orderByDesc = R => R.PricePerNight;
                        break;
                    default:
                        orderBy = R => R.Id;
                        break;
                }
            }
            else
                orderBy = r => r.Id;

            var rooms = await _unitOfWork.GetRepository<Room, int>()
                .GetAllAsync(filter, orderBy, orderByDesc);

            if (rooms is null || !rooms.Any())
            {
                genericResponse.StatusCode = StatusCodes.Status404NotFound;
                genericResponse.Message = "No Rooms Found";
                return genericResponse;
            }

            var roomsDto = _mapper.Map<IEnumerable<RoomDTO>>(rooms);
            genericResponse.StatusCode = StatusCodes.Status200OK;
            genericResponse.Message = "Rooms Retireved Successfuly";
            genericResponse.Data = roomsDto;

            return genericResponse;
        }

        public async Task<GenericResponse<RoomDetailsDto>> GetRoomDetailsAsync(int roomId)
        {

            var genericResponse = new GenericResponse<RoomDetailsDto>();

            Expression<Func<Room, bool>> filter = r => ((r.RoomStatus == RoomStatus.Available || r.RoomStatus == RoomStatus.Reserved));


            var room = await _unitOfWork
                .GetRepository<Room, int>()
                .GetByIdAsync(roomId, filter, [room => room.Images]);

            if (room is null)
            {
                genericResponse.StatusCode = StatusCodes.Status404NotFound;
                genericResponse.Message = "No Room Found";
                return genericResponse;
            }

            var roomDto = _mapper.Map<RoomDetailsDto>(room);
            genericResponse.StatusCode = StatusCodes.Status200OK;
            genericResponse.Message = "Room Retireved Successfuly";
            genericResponse.Data = roomDto;

            return genericResponse;
        }

        public async Task<GenericResponse<bool>> UpdateRoomAsync(int roomId, UpdateRoomDTO roomDTO)
        {
            var genericResponse = new GenericResponse<bool>();

            try
            {
                if (roomDTO is null)
                {
                    genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                    genericResponse.Message = "Room Can not be updated.";
                    return genericResponse;
                }

                var roomRepo = _unitOfWork.GetRepository<Room, int>();

                var room = await roomRepo.GetByIdAsync(roomId);

                if (room is null)
                {
                    genericResponse.StatusCode = StatusCodes.Status404NotFound;
                    genericResponse.Message = "Room is not found.";
                    return genericResponse;
                }


                _mapper.Map(roomDTO, room);

                room.UpdatedAt = DateTime.Now;

                roomRepo.Update(room);

                var result = await _unitOfWork.SaveChangesAsync() > 0;

                if (result)
                {
                    genericResponse.StatusCode = StatusCodes.Status200OK;
                    genericResponse.Message = "Room updated successfully.";
                    genericResponse.Data = true;
                }
                else
                {
                    genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                    genericResponse.Message = "Room is not updated.";
                    genericResponse.Data = false;
                }
                return genericResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while updating the room.");
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Message = "Room is not updated.";
                genericResponse.Data = false;
                return genericResponse;
            }


        }

        public async Task<GenericResponse<bool>> UploadImagesAsync(int roomId, List<IFormFile> files)
        {
            var genericResponse = new GenericResponse<bool>();

            try
            {
                var room = await _unitOfWork.GetRepository<Room, int>().GetByIdAsync(roomId);
                if (room is null)
                {
                    genericResponse.StatusCode = StatusCodes.Status404NotFound;
                    genericResponse.Message = "Room is not found";
                    return genericResponse;
                }

                foreach (var file in files)
                {
                    var fileName = await _attachmentService.UploadFileAsync(file, "rooms");
                    if (fileName is null)
                        continue;

                    var roomImage = new RoomImage()
                    {
                        RoomId = roomId,
                        ImageUrl = fileName
                    };
                    await _unitOfWork.GetRepository<RoomImage, int>().AddAsync(roomImage);
                }
                var result = await _unitOfWork.SaveChangesAsync() > 0;
                if (result)
                {
                    genericResponse.StatusCode = StatusCodes.Status200OK;
                    genericResponse.Message = "Images added successfully!";
                    genericResponse.Data = true;
                }

                else
                {
                    genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                    genericResponse.Message = "Images are not added!";
                }
                return genericResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An Error occored while addding the Images.");
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Message = "Images are not added.";
                genericResponse.Data = false;
                return genericResponse;

            }


        }
    }
}
