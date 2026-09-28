namespace WIL_Afcon.API.Services.Interfaces
{
    public interface IHikvisionService
    {
        Task<bool> DeleteEmployeeAsync(string deviceUserId);
    }
}