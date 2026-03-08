using HMS.Core.Contracts;
using HMS.Core.Entities.SecurityModule;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Infrastructure.Data.DataSeed
{
    public class IdentitydataInitializer(UserManager<HotelUser> _userManager,
        RoleManager<IdentityRole> _roleManager) : IDataInitializer
    {
        public async Task InitializeAdminAndRoleAsync()
        {
            if (!_roleManager.Roles.Any())
            {
                var adminRole = new IdentityRole() { Name = "Admin" };
                var staffRole = new IdentityRole() { Name = "Staff" };
                var guestRole = new IdentityRole() { Name = "Guest" };
                await _roleManager.CreateAsync(adminRole);
                await _roleManager.CreateAsync(staffRole);
                await _roleManager.CreateAsync(guestRole);
            }

            if (!_userManager.Users.Any())
            {
                var admin = new HotelUser()
                {
                    FullName = "Admin.HMS",
                    Email = "Admin.HMS@gmail.com",
                    UserName = "Admin_HMS",
                    CreatedAt = DateTime.Now
                };

                await _userManager.CreateAsync(admin, "P@ssw0rd");
                await _userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
