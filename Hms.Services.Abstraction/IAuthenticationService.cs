using HMS.Shared.DataTransferObjects.AuthDTOs;
using HMS.Shared.Respones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hms.Services.Abstraction
{
    public interface IAuthenticationService
    {
        Task<GenericResponse<UserDTO>> RegisterAsync(RegisterDTO registerData);
        Task<GenericResponse<UserDTO>> LoginAsync(LoginDTO loginData);
        Task<GenericResponse<bool>> CreateStaffAsync(StaffUserDTO staffData);
        Task<GenericResponse<IEnumerable<GetUserDTO>>> GetAllUsersForAdminAsync();
        Task<GenericResponse<bool>> ActivateUserAsync(string userId);
        Task<GenericResponse<bool>> DeactivateUserAsync(string userId);

        Task<GenericResponse<bool>> CheckEmailExistsAsync(string email);
        Task<GenericResponse<UserProfileDTO>> GetUserProfileAsync(string userId);

    }
}
