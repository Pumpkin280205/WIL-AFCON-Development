namespace WIL_Afcon.Models.ViewModels
{
    public class AdminProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = "Admin";

        public bool EmailConfirmed { get; set; }
    }
}