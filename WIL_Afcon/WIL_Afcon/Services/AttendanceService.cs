using WIL_Afcon.Models;
using WIL_Afcon.Repositories.Interfaces;
using WIL_Afcon.Services.Interfaces;

namespace WIL_Afcon.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;

        public AttendanceService(
            IAttendanceRepository attendanceRepository)
        {
            _attendanceRepository = attendanceRepository;
        }

        public async Task<List<Attendance>> GetAllAsync()
        {
            return await _attendanceRepository.GetAllAsync();
        }

        public async Task<Attendance?> GetByIdAsync(string id)
        {
            return await _attendanceRepository.GetByIdAsync(id);
        }

        public async Task<List<Attendance>> GetByEmployeeIdAsync(
            string employeeId)
        {
            return await _attendanceRepository
                .GetByEmployeeIdAsync(employeeId);
        }

        public async Task CreateAsync(Attendance attendance)
        {
            await _attendanceRepository.CreateAsync(attendance);
        }

        public async Task UpdateAsync(Attendance attendance)
        {
            await _attendanceRepository.UpdateAsync(attendance);
        }

        public async Task DeleteAsync(string id)
        {
            await _attendanceRepository.DeleteAsync(id);
        }
    }
}