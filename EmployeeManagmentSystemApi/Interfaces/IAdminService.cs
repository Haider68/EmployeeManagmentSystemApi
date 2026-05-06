using EmployeeManagmentSystemApi.DTOs.Admin;
using EmployeeManagmentSystemApi.Models;

namespace EmployeeManagmentSystemApi.Interfaces
{
    public record AuthResult(object User, string AccessToken, string RefreshToken, string UserType);

    public interface IAdminService
    {
        Task<AuthResult> RegisterAsync(RegisterAdminDto dto);
        Task<AuthResult> LoginAsync(LoginDto dto);
        Task<AuthResult> RefreshTokenAsync(string refreshToken);
        Task<object> GetMeAsync(int userId, string userType);
        Task<Admin> UpdateProfileAsync(int userId, UpdateAdminProfileDto dto);
        Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
    }
}
