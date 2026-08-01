using Drivious.Constants;
using Drivious.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Drivious.Data
{
    public static class DbSeeder
    {
        /// <summary>
        /// Brings the database up to the latest migration, then creates the
        /// application roles and the first administrator when one is configured.
        /// Safe to run on every start: everything here is idempotent.
        /// </summary>
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // On a machine that has never run this API there is no database yet, and
            // everything below would fail against it. Applying migrations first makes
            // a fresh checkout start on its own.
            await context.Database.MigrateAsync();

            foreach (var role in AppRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    logger.LogInformation("Created role {Role}.", role);
                }
            }

            var adminEmail = config["Seed:AdminEmail"];
            var adminPassword = config["Seed:AdminPassword"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                // Not an error: an environment may already have its administrator.
                logger.LogInformation(
                    "Seed:AdminEmail / Seed:AdminPassword are not configured, skipping admin creation.");
                return;
            }

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new AppUser
                {
                    UserName = config["Seed:AdminUserName"] ?? "admin",
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var created = await userManager.CreateAsync(admin, adminPassword);

                if (!created.Succeeded)
                {
                    logger.LogError("Could not create the seed administrator: {Errors}",
                        string.Join(", ", created.Errors.Select(x => x.Description)));
                    return;
                }

                logger.LogInformation("Created the seed administrator {Email}.", adminEmail);
            }

            if (!await userManager.IsInRoleAsync(admin, AppRoles.Admin))
            {
                await userManager.AddToRoleAsync(admin, AppRoles.Admin);
                logger.LogInformation("Granted {Email} the {Role} role.", adminEmail, AppRoles.Admin);
            }
        }
    }
}
