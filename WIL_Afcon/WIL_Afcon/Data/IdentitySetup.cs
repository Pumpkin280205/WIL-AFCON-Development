using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;

namespace WIL_Afcon.Data
{
    public static class IdentitySetup
    {
        public static async Task InitializeAsync(
            IServiceProvider services)

            // Reference: Microsoft (2026) 'ServiceProviderServiceExtensions.GetRequiredService Method',

            // Microsoft Learn.

            // Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.serviceproviderserviceextensions.getrequiredservice

            // (Accessed: 26 September 2026).
        {
            var roleManager =
                services.GetRequiredService<RoleManager<MongoRole>>();

            var userManager =
                services.GetRequiredService<UserManager<MongoUser>>();

            var adminExists =
                await roleManager.RoleExistsAsync("Admin");

            if (!adminExists)
            {
                var adminRole = new MongoRole("Admin");

                var result =
                    await roleManager.CreateAsync(adminRole);

                if (!result.Succeeded)
                {
                    throw new Exception(
                        "Failed to create Admin role: " +
                        string.Join(", ",
                            result.Errors.Select(e => e.Description)));
                }
            }

            var employeeExists =
    await roleManager.RoleExistsAsync("Employee");

            if (!employeeExists)
            {
                var employeeRole = new MongoRole("Employee");

                var result =
                    await roleManager.CreateAsync(employeeRole);

                if (!result.Succeeded)
                {
                    throw new Exception(
                        "Failed to create Employee role: " +
                        string.Join(", ",
                            result.Errors.Select(e => e.Description)));
                }
            }

            // Admin account details
            var adminEmail = "admin@afcon.com";
            var adminPassword = "AfconAdmin123!";

            // Check if the Admin user already exists

            // Reference: Microsoft (2026) 'UserManager<TUser>.FindByEmailAsync Method',

            // Microsoft Learn.

            // Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1.findbyemailasync

            // (Accessed: 26 September 2026).
            var adminUser =
                await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new MongoUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var userResult =
                    await userManager.CreateAsync(
                        adminUser,
                        adminPassword);

                if (!userResult.Succeeded)
                {
                    throw new Exception(
                        "Failed to create Admin user: " +
                        string.Join(", ",
                            userResult.Errors.Select(e => e.Description)));
                }
            }

            // Make sure the Admin user has the Admin role

            // Reference: Microsoft (2026) 'UserManager<TUser>.IsInRoleAsync Method',

            // Microsoft Learn.

            // Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1.isinroleasync

            // (Accessed: 26 September 2026)
            if (!await userManager.IsInRoleAsync(
                adminUser,
                "Admin"))
            {
                var roleResult =
                    await userManager.AddToRoleAsync(
                        adminUser,
                        "Admin");

                if (!roleResult.Succeeded)
                {
                    throw new Exception(
                        "Failed to assign Admin role: " +
                        string.Join(", ",
                            roleResult.Errors.Select(e => e.Description)));
                }
            }



            // Employee account details
            var employeeEmail = "employee@afcon.com";
            var employeePassword = "AfconEmployee123!";

            // Check if the Employee user already exists
            var employeeUser =
                await userManager.FindByEmailAsync(employeeEmail);

            if (employeeUser == null)
            {
                employeeUser = new MongoUser
                {
                    UserName = employeeEmail,
                    Email = employeeEmail,
                    EmailConfirmed = true
                };

                var userResult =
                    await userManager.CreateAsync(
                        employeeUser,
                        employeePassword);

                if (!userResult.Succeeded)
                {
                    throw new Exception(
                        "Failed to create Employee user: " +
                        string.Join(", ",
                            userResult.Errors.Select(e => e.Description)));
                }
            }

            // Make sure the Employee user has the Employee role
            if (!await userManager.IsInRoleAsync(
                employeeUser,
                "Employee"))
            {
                var roleResult =
                    await userManager.AddToRoleAsync(
                        employeeUser,
                        "Employee");

                if (!roleResult.Succeeded)
                {
                    throw new Exception(
                        "Failed to assign Employee role: " +
                        string.Join(", ",
                            roleResult.Errors.Select(e => e.Description)));
                }
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
// Microsoft (2026) 'UserManager<TUser>.CreateAsync Method',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1.createasync
// (Accessed: 26 September 2026).
//
// ============================================================
