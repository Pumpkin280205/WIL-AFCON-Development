using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WIL_Afcon.Services.Interfaces;    
namespace WIL_Afcon.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        public async Task<IActionResult> Index()
        {
            var attendance = await _attendanceService.GetAllAsync();

            return View(attendance);
        }
    }
}
