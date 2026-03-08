using Hms.Services.Abstraction;
using HMS.Core.Entities.SecurityModule;
using HMS.Shared.DataTransferObjects.AuthDTOs;
using HMS.Shared.Messages;
using HMS.Shared.Respones;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Services.Services
{
    public class AuthenticationService(UserManager<HotelUser> _userManager, IConfiguration _configuration
        , IEmailService _emailService) : IAuthenticationService
    {
        public async Task<GenericResponse<UserDTO>> RegisterAsync(RegisterDTO registerData)
        {
            var genericResponse = new GenericResponse<UserDTO>();

            if (registerData is null)
            {
                genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                genericResponse.Message = "Fill the data";
                return genericResponse;
            }

            var emailExists = await _userManager.FindByEmailAsync(registerData.Email);
            if (emailExists is not null)
            {
                genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                genericResponse.Message = "Email is used";
                return genericResponse;
            }

            var user = new HotelUser()
            {
                Email = registerData.Email,
                FullName = registerData.FullName,
                PhoneNumber = registerData.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UserName = registerData.Email.Split('@')[0]
            };

            var result = await _userManager.CreateAsync(user, registerData.Password);

            if (result.Succeeded)
            {
                var email = new Email()
                {
                    To = registerData.Email,
                    Subject = $"Welcome {registerData.FullName} to our HotelSystem Application.",
                    Body = "A welcome message from HotelSystem Support, Login and enjoy our Hotel Services."
                };

                await _emailService.SendEmail(email);

                await _userManager.AddToRoleAsync(user, "Guest");
                genericResponse.StatusCode = StatusCodes.Status200OK;
                genericResponse.Message = "Account Created successfully";
                genericResponse.Data = new UserDTO()
                {
                    Email = registerData.Email,
                    FullName = registerData.FullName,
                    Token = await CreateTokenAsync(user)
                };
            }
            else
            {
                genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                genericResponse.Message = string.Join(" | ", result.Errors.Select(e => e.Description));
            }
            return genericResponse;
        }

        public async Task<GenericResponse<UserDTO>> LoginAsync(LoginDTO loginData)
        {
            var genericResponse = new GenericResponse<UserDTO>();

            if (loginData is null)
            {
                genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                genericResponse.Message = "Fill the data.";
                return genericResponse;
            }

            var user = await _userManager.FindByEmailAsync(loginData.Email);

            if (user is null)
            {
                genericResponse.StatusCode = StatusCodes.Status401Unauthorized;
                genericResponse.Message = "Invalid Email or Password";
                return genericResponse;
            }

            if (!user.IsActive)
            {
                genericResponse.StatusCode = StatusCodes.Status403Forbidden;
                genericResponse.Message = "Account has been Deactivated.";
                return genericResponse;
            }

            var result = await _userManager.CheckPasswordAsync(user, loginData.Password);

            if (!result)
            {
                genericResponse.StatusCode = StatusCodes.Status401Unauthorized;
                genericResponse.Message = "Invalid Email or Password";
            }
            else
            {
                genericResponse.StatusCode = StatusCodes.Status200OK;
                genericResponse.Message = "Login Successfully.";
                genericResponse.Data = new UserDTO()
                {
                    Email = loginData.Email,
                    FullName = user.FullName,
                    Token = await CreateTokenAsync(user)
                };
            }
            return genericResponse;
        }

        public async Task<GenericResponse<bool>> CreateStaffAsync(StaffUserDTO staffData)
        {
            var genericRespone = new GenericResponse<bool>();

            if (staffData is null)
            {
                genericRespone.StatusCode = StatusCodes.Status400BadRequest;
                genericRespone.Message = "Fill up the data.";
                return genericRespone;
            }

            var checkEmailExists = await _userManager.FindByEmailAsync(staffData.Email);
            if (checkEmailExists is not null)
            {
                genericRespone.StatusCode = StatusCodes.Status400BadRequest;
                genericRespone.Message = "Email already Exists.";
                return genericRespone;
            }

            var isParsed = Enum.TryParse(staffData.Specialities, out StaffSpecialities staffSpecialities);
            if (!isParsed)
            {
                genericRespone.StatusCode = StatusCodes.Status400BadRequest;
                genericRespone.Message = "Invalid staff speciality.";
                return genericRespone;
            }

            var staffUser = new StaffUser()
            {
                Email = staffData.Email,
                FullName = staffData.FullName,
                PhoneNumber = staffData.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.Now,
                Specialities = staffSpecialities,
                UserName = staffData.Email.Split('@')[0]
            };

            var result = await _userManager.CreateAsync(staffUser);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(staffUser, "Staff");
                genericRespone.StatusCode = StatusCodes.Status200OK;
                genericRespone.Message = "Staff created successfully.";
                genericRespone.Data = true;
            }
            else
            {
                genericRespone.StatusCode = StatusCodes.Status400BadRequest;
                genericRespone.Message = string.Join(" | ", result.Errors.Select(e => e.Description));
            }
            return genericRespone;
        }

        public async Task<GenericResponse<IEnumerable<GetUserDTO>>> GetAllUsersForAdminAsync()
        {
            var genericResponse = new GenericResponse<IEnumerable<GetUserDTO>>();

            var users = await _userManager.Users.ToListAsync();
            if (users is null)
            {
                genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                genericResponse.Message = "There are no users found";
                return genericResponse;
            }

            var listOfUsersToReturn = new List<GetUserDTO>();
            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    continue;

                var role = await _userManager.GetRolesAsync(user);
                var userToReturnDTO = new GetUserDTO()
                {
                    Email = user.Email!,
                    Id = user.Id,
                    IsActive = user.IsActive,
                    Role = role.FirstOrDefault()!
                };
                listOfUsersToReturn.Add(userToReturnDTO);
            }

            genericResponse.StatusCode = StatusCodes.Status200OK;
            genericResponse.Message = "Users have been return successfully";
            genericResponse.Data = listOfUsersToReturn;
            return genericResponse;
        }

        public async Task<GenericResponse<bool>> ActivateUserAsync(string userId)
        {
            var genericResponse = new GenericResponse<bool>();

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                genericResponse.StatusCode = StatusCodes.Status404NotFound;
                genericResponse.Message = "User is not found";
                return genericResponse;
            }

            user.IsActive = true;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Message = string.Join(" | ", result.Errors.Select(e => e.Description));
            }
            else
            {
                genericResponse.StatusCode = StatusCodes.Status200OK;
                genericResponse.Message = "User has been activated.";
                genericResponse.Data = true;
            }
            return genericResponse;
        }

        public async Task<GenericResponse<bool>> DeactivateUserAsync(string userId)
        {
            var genericResponse = new GenericResponse<bool>();

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                genericResponse.StatusCode = StatusCodes.Status404NotFound;
                genericResponse.Message = "User is not found";
                return genericResponse;
            }

            user.IsActive = false;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Message = string.Join(" | ", result.Errors.Select(e => e.Description));
            }
            else
            {
                genericResponse.StatusCode = StatusCodes.Status200OK;
                genericResponse.Message = "User has been deactivated.";
                genericResponse.Data = true;
            }
            return genericResponse;
        }

        public async Task<GenericResponse<bool>> CheckEmailExistsAsync(string email)
        {
            var genericResponse = new GenericResponse<bool>();

            var user = await _userManager.FindByEmailAsync(email);

            if (user is not null)
            {
                genericResponse.StatusCode = StatusCodes.Status200OK;
                genericResponse.Message = "Email exists";
                genericResponse.Data = true;
            }
            else
            {
                genericResponse.StatusCode = StatusCodes.Status200OK;
                genericResponse.Message = "Email does not exist";
                genericResponse.Data = false;
            }
            return genericResponse;
        }

        public async Task<GenericResponse<UserProfileDTO>> GetUserProfileAsync(string userId)
        {
            var genericResponse = new GenericResponse<UserProfileDTO>();

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                genericResponse.StatusCode = StatusCodes.Status404NotFound;
                genericResponse.Message = "Profile is not Found.";
                return genericResponse;
            }

            var profileToReturn = new UserProfileDTO()
            {
                Email = user.Email!,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber!,
                UserName = user.UserName!
            };
            genericResponse.StatusCode = StatusCodes.Status200OK;
            genericResponse.Message = "Profile is retrieved successfully";
            genericResponse.Data = profileToReturn;

            return genericResponse;
        }
        private async Task<string> CreateTokenAsync(HotelUser user)
        {
            var claims = new List<Claim>()
            {
                new (JwtRegisteredClaimNames.Email, user.Email!),
                new (JwtRegisteredClaimNames.NameId, user.Id!),
                new("Activity",user.IsActive.ToString()!)
            };
            var Roles = await _userManager.GetRolesAsync(user);

            foreach (var role in Roles)
                claims.Add(new(ClaimTypes.Role, role));

            var SecretKey = _configuration["JwtOptions:SecretKey"];

            var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey!));

            var cred = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);

            var Token = new JwtSecurityToken(
                issuer: _configuration["JwtOptions:Issuer"],
                audience: _configuration["JwtOptions:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: cred
                );

            return new JwtSecurityTokenHandler().WriteToken(Token);
        }
    }
}
