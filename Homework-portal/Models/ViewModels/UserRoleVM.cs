using System.Collections.Generic;

namespace Homework_portal.Models.ViewModels
{
    public class UserRoleVM
    {
        public string UserId { get; set; } = string.Empty;
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CurrentRole { get; set; } = string.Empty;
        public List<string> AvailableRoles { get; set; } = new();

        // Yeni: sýnýf/þube ve öðrenci no bilgisi
        public string? Sinif { get; set; }
        public string? Sube { get; set; }
        public string? OgrenciNo { get; set; }
    }
}
