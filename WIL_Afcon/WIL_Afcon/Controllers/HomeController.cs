using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WIL_Afcon.Models;

namespace WIL_Afcon.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}


// Reference: Microsoft (2026) 'IActionResult Interface',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.iactionresult
// (Accessed: 26 September 2026).
