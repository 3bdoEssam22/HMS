using Hms.Services.Abstraction;
using HMS.Core.Entities.SecurityModule;
using HMS.Shared.DataTransferObjects.AuthDTOs;
using HMS.Shared.Respones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HMS.Api.Controllers
{
    public class AuthController(IAuthenticationService _authenticationService) : BaseApiController
    {
        //Post BaseUrl/api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<GenericResponse<UserDTO>>> Register([FromBody] RegisterDTO registerDTO)
        {
            var result = await _authenticationService.RegisterAsync(registerDTO);
            return HandleRespone(result);
        }

        //Post BaseUrl/api/Auth/Login
        [HttpPost("Login")]
        public async Task<ActionResult<GenericResponse<UserDTO>>> Login([FromBody] LoginDTO loginDTO)
        {
            var result = await _authenticationService.LoginAsync(loginDTO);
            return HandleRespone(result);
        }

        //Post BaseUrl/api/Auth/Create-Staff
        [Authorize(Roles = "Admin")]
        [HttpPost("Create-Staff")]
        public async Task<ActionResult<GenericResponse<bool>>> CreateStaffAccountAsync(StaffUserDTO staffUser)
        {
            var result = await _authenticationService.CreateStaffAsync(staffUser);
            return HandleRespone(result);
        }

        //Get /BaseUrl/api/Auth/users
        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<ActionResult<GenericResponse<IEnumerable<GetUserDTO>>>> GetAllUsersForAdminAsync()
        {
            var result = await _authenticationService.GetAllUsersForAdminAsync();
            return HandleRespone(result);
        }

        //Put BaseUrl/api/Auth/user/{id}/activate
        [Authorize(Roles = "Admin")]
        [HttpPut("users/{id}/activate")]
        public async Task<ActionResult<GenericResponse<bool>>> ActivateUserAsync(string id)
        {
            var result = await _authenticationService.ActivateUserAsync(id);
            return HandleRespone(result);
        }

        //Put BaseUrl/api/Auth/user/{id}/deactivate
        [Authorize(Roles = "Admin")]
        [HttpPut("users/{id}/deactivate")]
        public async Task<ActionResult<GenericResponse<bool>>> DeactivateUserAsync(string id)
        {
            var result = await _authenticationService.DeactivateUserAsync(id);
            return HandleRespone(result);
        }

        //Get BaseUrl/api/Auth/emailExists
        [HttpGet("emailExists")]
        public async Task<ActionResult<GenericResponse<bool>>> CheckEmail(string email)
        {
            var user = await _authenticationService.CheckEmailExistsAsync(email);
            return HandleRespone(user);
        }

        //Get BaseUrl/api/Auth/profile
        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<GenericResponse<UserProfileDTO>>> GetUserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _authenticationService.GetUserProfileAsync(userId!);
            return HandleRespone(result);
        }

    }
}
