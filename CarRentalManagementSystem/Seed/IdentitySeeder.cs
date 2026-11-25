using Microsoft.AspNetCore.Identity;

namespace CarRentalManagementSystem.Seed
{
    // I use this class to create the roles and a default admin account automatically.
    // This saves time because I don't want to manually set roles every time.
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string[] roles = { "Admin", "Customer" };

            // Create roles if they don't exist
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // The admin account details come from configuration (user secrets or environment
            // variables), so no password is stored in the source code.
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var adminEmail = config["SeedAdmin:Email"];
            var adminPassword = config["SeedAdmin:Password"];

            // Skip creating an admin if the details haven't been configured
            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                return;
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            // If the admin user doesn't exist yet, create it
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);

                // If creation worked, assign Admin role
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else
            {
                // If the user exists, make sure it's in the Admin role
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}

