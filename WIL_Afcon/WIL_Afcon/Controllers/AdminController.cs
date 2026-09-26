using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WIL_Afcon.Controllers
{

// Reference: Microsoft (2026) 'Role-based authorization in ASP.NET Core MVC',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/roles
// (Accessed: 26 September 2026).
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {

   // Reference: Microsoft (2026) 'IActionResult Interface',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.iactionresult
// (Accessed: 26 September 2026). 
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}



//Microsoft (2026) 'IActionResult Interface', Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.iactionresult (Accessed: 26 September 2026).

//Microsoft (2026) 'Role-based authorization in ASP.NET Core MVC', Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/roles (Accessed: 26 September 2026).
