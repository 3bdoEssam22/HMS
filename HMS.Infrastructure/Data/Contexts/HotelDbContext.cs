using HMS.Core.Entities.BookingModule;
using HMS.Core.Entities.SecurityModule;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Infrastructure.Data.Contexts
{
    public class HotelDbContext(DbContextOptions<HotelDbContext> options) : IdentityDbContext<HotelUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<HotelUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<StaffUser>().ToTable("StaffUser");

            modelBuilder.Entity<Booking>().Property(b => b.TotalAmount).HasPrecision(8, 2);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
