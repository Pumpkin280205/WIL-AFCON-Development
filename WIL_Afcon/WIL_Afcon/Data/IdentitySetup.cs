using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;

namespace WIL_Afcon.Data
{
    public static class IdentitySetup
    {
        // Creates the roles, and optionally the seed Admin / Employee accounts.
        // Seed account credentials come from configuration ("SeedAccounts"
        // section - user-secrets locally, environment variables when hosted),
        // never from source code. An account is skipped when its password is
        // not configured.
        public static async Task InitializeAsync(
            IServiceProvider services)
        {
            var roleManager =
                services.GetRequiredService<RoleManager<MongoRole>>();

            var userManager =
                services.GetRequiredService<UserManager<MongoUser>>();

            var configuration =
                services.GetRequiredService<IConfiguration>();

            await EnsureRoleAsync(roleManager, "Admin");
            await EnsureRoleAsync(roleManager, "Employee");

            await EnsureUserAsync(
                userManager,
                configuration["SeedAccounts:AdminEmail"],
                configuration["SeedAccounts:AdminPassword"],
                "Admin");

            await EnsureUserAsync(
                userManager,
                configuration["SeedAccounts:EmployeeEmail"],
                configuration["SeedAccounts:EmployeePassword"],
                "Employee");
        }

        private static async Task EnsureRoleAsync(
            RoleManager<MongoRole> roleManager,
            string roleName)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                return;
            }

            var result =
                await roleManager.CreateAsync(new MongoRole(roleName));

            ThrowIfFailed(result, $"Failed to create {roleName} role");
        }

        private static async Task EnsureUserAsync(
            UserManager<MongoUser> userManager,
            string? email,
            string? password,
            string roleName)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new MongoUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var userResult =
                    await userManager.CreateAsync(user, password);

                ThrowIfFailed(userResult, $"Failed to create {roleName} user");
            }

            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                var roleResult =
                    await userManager.AddToRoleAsync(user, roleName);

                ThrowIfFailed(roleResult, $"Failed to assign {roleName} role");
            }
        }

        private static void ThrowIfFailed(IdentityResult result, string message)
        {
            if (!result.Succeeded)
            {
                throw new Exception(
                    message + ": " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}



// Microsoft (2026) 'RoleManager<TRole>.CreateAsync Method',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.rolemanager-1.createasync
// (Accessed: 26 September 2026).
//
// Microsoft (2026) 'RoleManager<TRole>.RoleExistsAsync Method',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.rolemanager-1.roleexistsasync
// (Accessed: 26 September 2026).
//
// Microsoft (2026) 'ServiceProviderServiceExtensions.GetRequiredService Method',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.serviceproviderserviceextensions.getrequiredservice
// (Accessed: 26 September 2026).
//
// Microsoft (2026) 'UserManager<TUser>.CreateAsync Method',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1.createasync
// (Accessed: 26 September 2026).
//
// Microsoft (2026) 'UserManager<TUser>.FindByEmailAsync Method',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1.findbyemailasync
// (Accessed: 26 September 2026).
//
// Microsoft (2026) 'UserManager<TUser>.IsInRoleAsync Method',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1.isinroleasync
// (Accessed: 26 September 2026).
//
// ============================================================
