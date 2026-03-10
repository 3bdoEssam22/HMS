using Hms.Services.Abstraction;
using HMS.Api.Extensions;
using HMS.Core.Contracts;
using HMS.Core.Entities.SecurityModule;
using HMS.Infrastructure.Data.Contexts;
using HMS.Infrastructure.Data.DataSeed;
using HMS.Infrastructure.ExternalService;
using HMS.Infrastructure.Repository;
using HMS.Services;
using HMS.Services.Helpers;
using HMS.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace HMS.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<HotelDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IRoomService, RoomService>();
            builder.Services.AddAutoMapper(typeof(ServicesAssemblyReference).Assembly);
            builder.Services.AddTransient<IAttachmentService, AttachmentService>();
            builder.Services.AddScoped<IDataInitializer, IdentitydataInitializer>();
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();


            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.TokenValidationParameters = new()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["JwtOptions:Issuer"],
                        ValidAudience = builder.Configuration["JwtOptions:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtOptions:SecretKey"]!))
                    };
                }
                );


            builder.Services.AddIdentityCore<HotelUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<HotelDbContext>();

            builder.Services.Configure<EmailSettings>(
                builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddTransient<IEmailService, EmailService>();

            builder.Services.AddScoped<IBookingService, BookingService>();

            builder.Services.AddHttpClient<IPaymentService, PaymentService>();

            #endregion            

            var app = builder.Build();

            await app.MigrateDatabaseAsync();
            await app.SeedingIdentityData();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
