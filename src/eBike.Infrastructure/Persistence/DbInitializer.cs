using eBike.Application.Common.Interfaces;
using eBike.Application.Common.Security;
using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eBike.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        ApplicationDbContext context, 
        ICryptographyService cryptoService, 
        ILogger logger)
    {
        try
        {
            // Seed Admin Role if not exists
            if (!await context.Roles.AnyAsync(r => r.Name == "Admin"))
            {
                context.Roles.Add(new ApplicationRole
                {
                    Name = "Admin"
                });
                await context.SaveChangesAsync();
            }

            // Seed SuperAdmin User if not exists
            var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "superadmin");

            if (adminUser == null)
            {
                var (hash, _) = cryptoService.CreatePasswordHash("Admin@123456");

                adminUser = new ApplicationUser
                {
                    UserName = "superadmin",
                    Email = "admin@ebike.com",
                    PasswordHash = hash,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    PhoneNumber = "01700000000",
                    EmailConfirmed = true,
                    ConcernID = 1,
                    EmployeeID = 1
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();

                context.UserRoles.Add(new ApplicationUserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });
                await context.SaveChangesAsync();

                logger.LogInformation("Database seeded with default Admin user (Username: superadmin).");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
        }
    }
}
