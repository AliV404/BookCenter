using BookCenter.Application.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookCenter.Persistence.Identity
{
    public static class IdentitySeeder
    {
        /// <summary>
        /// Creates the roles and, when Seed:AdminEmail / Seed:AdminPassword are configured,
        /// the first admin account. Safe to run on every startup.
        /// </summary>
        public static async Task SeedIdentityAsync(this IServiceProvider services, IConfiguration configuration)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            foreach (var role in Roles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var email = configuration["Seed:AdminEmail"];
            var password = configuration["Seed:AdminPassword"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return;

            if (await userManager.FindByEmailAsync(email) is not null)
                return;

            var admin = new AppUser { UserName = email, Email = email, FullName = "Administrator", EmailConfirmed = true };
            var created = await userManager.CreateAsync(admin, password);
            if (!created.Succeeded)
                throw new InvalidOperationException("Could not create the seed admin: " + string.Join(" ", created.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}
