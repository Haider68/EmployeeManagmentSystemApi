using BCrypt.Net;
using EmployeeManagmentSystemApi.Data;
using EmployeeManagmentSystemApi.DTOs.Admin;
using EmployeeManagmentSystemApi.Helpers;
using EmployeeManagmentSystemApi.Interfaces;
using EmployeeManagmentSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagmentSystemApi.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _db;
        private readonly IJwtService _jwt;

        public AdminService(AppDbContext db, IJwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        public async Task<AuthResult> RegisterAsync(RegisterAdminDto dto)
        {
            var existing = await _db.Admins.FirstOrDefaultAsync(a => a.Email == dto.Email);
            if (existing != null)
                throw new Exception("Email already exists");

            var admin = new Admin
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNo = dto.PhoneNo,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "admin"
            };

            _db.Admins.Add(admin);
            await _db.SaveChangesAsync();

            var accessToken = _jwt.GenerateAccessToken(admin.Id, admin.Email, admin.Role, "admin");
            var refreshToken = _jwt.GenerateRefreshToken(admin.Id, admin.Email, admin.Role, "admin");

            return new AuthResult(admin, accessToken, refreshToken, "admin");
        }

        public async Task<AuthResult> LoginAsync(LoginDto dto)
        {
            // Try admin first, then employee
            var admin = await _db.Admins.FirstOrDefaultAsync(a => a.Email == dto.Email);
            if (admin != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash))
                    throw new Exception("Incorrect Password");

                var access = _jwt.GenerateAccessToken(admin.Id, admin.Email, admin.Role, "admin");
                var refresh = _jwt.GenerateRefreshToken(admin.Id, admin.Email, admin.Role, "admin");
                return new AuthResult(admin, access, refresh, "admin");
            }

            var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Email == dto.Email);
            if (employee == null)
                throw new Exception("User not found");

            if (string.IsNullOrEmpty(employee.PasswordHash) || !BCrypt.Net.BCrypt.Verify(dto.Password, employee.PasswordHash))
                throw new Exception("Incorrect Password");

            var empAccess = _jwt.GenerateAccessToken(employee.Id, employee.Email, employee.Role, "employee");
            var empRefresh = _jwt.GenerateRefreshToken(employee.Id, employee.Email, employee.Role, "employee");
            return new AuthResult(employee, empAccess, empRefresh, "employee");
        }

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            var principal = _jwt.ValidateToken(refreshToken)
                ?? throw new Exception("Invalid or expired refresh token");

            var userId = int.Parse(principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst("sub")?.Value
                ?? throw new Exception("Invalid token"));

            var userType = principal.FindFirst("userType")?.Value ?? "employee";

            if (userType == "admin")
            {
                var admin = await _db.Admins.FindAsync(userId)
                    ?? throw new Exception("User not found");

                var access = _jwt.GenerateAccessToken(admin.Id, admin.Email, admin.Role, "admin");
                var refresh = _jwt.GenerateRefreshToken(admin.Id, admin.Email, admin.Role, "admin");
                return new AuthResult(admin, access, refresh, "admin");
            }
            else
            {
                var employee = await _db.Employees.FindAsync(userId)
                    ?? throw new Exception("User not found");

                var access = _jwt.GenerateAccessToken(employee.Id, employee.Email, employee.Role, "employee");
                var refresh = _jwt.GenerateRefreshToken(employee.Id, employee.Email, employee.Role, "employee");
                return new AuthResult(employee, access, refresh, "employee");
            }
        }

        public async Task<object> GetMeAsync(int userId, string userType)
        {
            if (userType == "admin")
            {
                var admin = await _db.Admins.FindAsync(userId)
                    ?? throw new Exception("User not found");
                return admin;
            }
            else
            {
                var employee = await _db.Employees.FindAsync(userId)
                    ?? throw new Exception("User not found");
                return employee;
            }
        }

        public async Task<Admin> UpdateProfileAsync(int userId, UpdateAdminProfileDto dto)
        {
            var admin = await _db.Admins.FindAsync(userId)
                ?? throw new Exception("User not found");

            if (!string.IsNullOrWhiteSpace(dto.FullName)) admin.FullName = dto.FullName;
            if (!string.IsNullOrWhiteSpace(dto.PhoneNo)) admin.PhoneNo = dto.PhoneNo;
            admin.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return admin;
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            if (dto.CurrentPassword == dto.NewPassword)
                throw new Exception("New password must be different from current password");

            var admin = await _db.Admins.FindAsync(userId)
                ?? throw new Exception("User not found");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, admin.PasswordHash))
                throw new Exception("Current password is incorrect");

            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            admin.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }
    }
}
