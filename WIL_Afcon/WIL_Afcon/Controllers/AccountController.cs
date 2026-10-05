using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WIL_Afcon.Services.Interfaces;
using WIL_Afcon.Models.ViewModels;

namespace WIL_Afcon.Controllers
{
    public class AccountController : Controller
    {

    // Reference: Microsoft (2026) 'Handle requests with controllers in ASP.NET Core MVC',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
// (Accessed: 26 September 2026).
        private readonly SignInManager<MongoUser> _signInManager;
        private readonly UserManager<MongoUser> _userManager;
        private readonly IEmployeeService _employeeService;

// Reference: Microsoft (2026) 'Dependency injection into controllers in ASP.NET Core',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/dependency-injection
// (Accessed: 26 September 2026).
        public AccountController(
      SignInManager<MongoUser> signInManager,
      UserManager<MongoUser> userManager,
      IEmployeeService employeeService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _employeeService = employeeService;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // POST: /Account/Login

        // Reference: Microsoft (2026) 'Prevent Cross-Site Request Forgery (XSRF/CSRF) attacks
// in ASP.NET Core', Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery
// (Accessed: 26 September 2026).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string password,
            bool rememberMe = false)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "",
                    "Email and password are required.");

                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(

            // Reference: Microsoft (2026) 'SignInManager<TUser>.PasswordSignInAsync Method',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.signinmanager-1.passwordsigninasync
// (Accessed: 26 September 2026).
                user.UserName!,
                password,
                rememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction(
                        "Dashboard",
                        "Admin");
                }

                if (await _userManager.IsInRoleAsync(user, "Employee"))
                {
                    return RedirectToAction(
                        "Dashboard",
                        "Employee");
                }

                await _signInManager.SignOutAsync();

                ModelState.AddModelError(
                    "",
                    "Your account does not have a valid role.");

                return View();
            }

            ModelState.AddModelError(
                "",
                "Invalid email or password.");

            return View();
        }



        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {

        // Reference: Microsoft (2026) 'Model validation in ASP.NET Core MVC',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation
// (Accessed: 26 September 2026).
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check that an Admin has already created this employee
            var employee = await _employeeService.GetByEmailAsync(model.Email);

            if (employee == null)
            {
                ModelState.AddModelError(
                    "",
                    "No employee account has been created for this email.");

                return View(model);
            }

            // Check whether this employee has already registered
            if (!string.IsNullOrEmpty(employee.UserId))
            {
                ModelState.AddModelError(
                    "",
                    "This employee has already registered.");

                return View(model);
            }

            // Check whether an Identity account already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "",
                    "An account with this email already exists.");

                return View(model);
            }

            // Create the Identity account
            var user = new MongoUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(
                user,
                model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            // Give the user the Employee role

            // Reference: Microsoft (2026) 'UserManager<TUser>.IsInRoleAsync Method',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1.isinroleasync
// (Accessed: 26 September 2026).

            
            var roleResult = await _userManager.AddToRoleAsync(
                user,
                "Employee");

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            // Automatically link the Identity account to the Employee
            employee.UserId = user.Id.ToString();

            await _employeeService.UpdateAsync(employee);

            return RedirectToAction(nameof(Login));
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(nameof(Login));
        }
    }
}

//Microsoft (2026) 'Dependency injection into controllers in ASP.NET Core', Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/dependency-injection (Accessed: 26 September 2026).

//Microsoft (2026) 'Handle requests with controllers in ASP.NET Core MVC', Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions (Accessed: 26 September 2026).

//Microsoft (2026) 'Model validation in ASP.NET Core MVC', Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation (Accessed: 26 September 2026).

//Microsoft (2026) 'Prevent Cross-Site Request Forgery (XSRF/CSRF) attacks in ASP.NET Core', Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery (Accessed: 26 September 2026).

//Microsoft (2026) 'SignInManager<TUser>.PasswordSignInAsync Method', Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.signinmanager-1.passwordsigninasync (Accessed: 26 September 2026).

//Microsoft (2026) 'UserManager<TUser>.AddToRoleAsync Method', Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1.addtoroleasync (Accessed: 26 September 2026).

//Microsoft (2026) 'UserManager<TUser>.CreateAsync Method', Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1.createasync (Accessed: 26 September 2026).

//Microsoft (2026) 'UserManager<TUser>.IsInRoleAsync Method', Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1.isinroleasync (Accessed: 26 September 2026).
