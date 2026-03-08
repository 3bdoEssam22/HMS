using HMS.Core.Contracts;
using HMS.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace HMS.Api.Extensions
{
    public static class WebApplicationRegiter
    {
        public static async Task<WebApplication> MigrateDatabaseAsync(this WebApplication app)
        {
            await using var scope = app.Services.CreateAsyncScope();
            var hotelDbContext = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            var pendingMigrations = await hotelDbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
                await hotelDbContext.Database.MigrateAsync();
            return app;
        }


        public static async Task<WebApplication> SeedingIdentityData(this WebApplication app)
        {
            await using var scope = app.Services.CreateAsyncScope();
            var dataInitializer = scope.ServiceProvider.GetRequiredService<IDataInitializer>();
            await dataInitializer.InitializeAdminAndRoleAsync();
            return app;
        }

    }
}
