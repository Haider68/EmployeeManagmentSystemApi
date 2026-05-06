using System.ComponentModel.DataAnnotations;

namespace EmployeeManagmentSystemApi.DTOs.Admin
{
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
