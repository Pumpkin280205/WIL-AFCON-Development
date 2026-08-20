using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;

namespace WIL_Afcon.Data
{
    public static class IdentitySetup
    {
        public static async Task InitializeAsync(
     IServiceProvider services)
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
        }
    }
}