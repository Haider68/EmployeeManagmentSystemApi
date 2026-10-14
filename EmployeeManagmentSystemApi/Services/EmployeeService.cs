using BCrypt.Net;
using EmployeeManagmentSystemApi.Data;
using EmployeeManagmentSystemApi.DTOs.Admin;
using EmployeeManagmentSystemApi.DTOs.Employee;
using EmployeeManagmentSystemApi.Interfaces;
using EmployeeManagmentSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagmentSystemApi.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _db;

        public EmployeeService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Employee> CreateEmployeeAsync(CreateEmployeeDto dto)
        {
            var existing = await _db.Employees.FirstOrDefaultAsync(e => e.Email == dto.Email);
            if (existing != null)
                throw new Exception("Email already exists");

            // Convert fingerprint image file → base64 if provided as a file upload
            string? fingerprintData = dto.FingerPrint;
            if (dto.FingerprintFile != null && dto.FingerprintFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await dto.FingerprintFile.CopyToAsync(ms);
                fingerprintData = Convert.ToBase64String(ms.ToArray());
            }

            // Convert employee photo file → base64 if provided as a file upload
            string? imageData = dto.Image;
            if (dto.Avatar != null && dto.Avatar.Length > 0 && string.IsNullOrEmpty(imageData))
            {
                using var ms = new MemoryStream();
                await dto.Avatar.CopyToAsync(ms);
                var base64 = Convert.ToBase64String(ms.ToArray());
                var mimeType = dto.Avatar.ContentType;
                imageData = $"data:{mimeType};base64,{base64}";
            }

            var employee = new Employee
            {
                FullName = dto.FullName,
                Position = dto.Position,
                Department = dto.Department,
                Email = dto.Email,
                Phone = dto.Phone,
                JoinDate = dto.JoinDate,
                ValidUntil = dto.ValidUntil,
                Status = dto.Status,
                BirthDate = dto.BirthDate,
                Address = dto.Address,
                Role = dto.Role,
                Salary = dto.Salary,
                HourlyRate = dto.HourlyRate,
                RequiredHoursPerMonth = dto.RequiredHoursPerMonth,
                OvertimeRate = dto.OvertimeRate,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Image = imageData,
                FingerPrint = fingerprintData
            };

            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();
            return employee;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync(string? status, string? department)
        {
            var query = _db.Employees.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "undefined")
                query = query.Where(e => e.Status == status);

            if (!string.IsNullOrWhiteSpace(department) && department != "undefined")
                query = query.Where(e => e.Department == department);

            return await query.OrderByDescending(e => e.CreatedAt).ToListAsync();
        }
        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
        {
            var employees = await _db.Employees.ToListAsync();
            var mapEmployees = Mapemployee(employees);
            return mapEmployees;
        }
        public async Task<Employee> GetEmployeeByIdAsync(int id)
        {
            return await _db.Employees.FindAsync(id)
                ?? throw new Exception("Employee not found");
        }

        public async Task<Employee> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto)
        {
            var employee = await _db.Employees.FindAsync(id)
                ?? throw new Exception("Employee not found");

            if (!string.IsNullOrWhiteSpace(dto.FullName)) employee.FullName = dto.FullName;
            if (!string.IsNullOrWhiteSpace(dto.Position)) employee.Position = dto.Position;
            if (!string.IsNullOrWhiteSpace(dto.Department)) employee.Department = dto.Department;
            if (!string.IsNullOrWhiteSpace(dto.Phone)) employee.Phone = dto.Phone;
            if (!string.IsNullOrWhiteSpace(dto.Address)) employee.Address = dto.Address;
            if (dto.JoinDate.HasValue) employee.JoinDate = dto.JoinDate.Value;
            if (dto.ValidUntil.HasValue) employee.ValidUntil = dto.ValidUntil.Value;
            if (dto.BirthDate.HasValue) employee.BirthDate = dto.BirthDate.Value;
            if (dto.Salary.HasValue) employee.Salary = dto.Salary.Value;
            if (dto.HourlyRate.HasValue) employee.HourlyRate = dto.HourlyRate.Value;
            if (dto.RequiredHoursPerMonth.HasValue) employee.RequiredHoursPerMonth = dto.RequiredHoursPerMonth.Value;
            if (dto.OvertimeRate.HasValue) employee.OvertimeRate = dto.OvertimeRate.Value;
            if (!string.IsNullOrWhiteSpace(dto.AvatarUrl)) employee.AvatarUrl = dto.AvatarUrl;
            if (!string.IsNullOrWhiteSpace(dto.AvatarPublicId)) employee.AvatarPublicId = dto.AvatarPublicId;

            // Handle fingerprint file upload → base64
            if (dto.FingerprintFile != null && dto.FingerprintFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await dto.FingerprintFile.CopyToAsync(ms);
                employee.FingerPrint = Convert.ToBase64String(ms.ToArray());
            }
            else if (!string.IsNullOrWhiteSpace(dto.FingerPrint))
            {
                employee.FingerPrint = dto.FingerPrint;
            }

            // Handle employee photo file upload → base64
            if (dto.Avatar != null && dto.Avatar.Length > 0)
            {
                using var ms = new MemoryStream();
                await dto.Avatar.CopyToAsync(ms);
                var mimeType = dto.Avatar.ContentType;
                employee.Image = $"data:{mimeType};base64,{Convert.ToBase64String(ms.ToArray())}";
            }
            else if (!string.IsNullOrWhiteSpace(dto.Image))
            {
                employee.Image = dto.Image;
            }

            employee.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return employee;
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            var employee = await _db.Employees.FindAsync(id)
                ?? throw new Exception("Employee not found");

            _db.Employees.Remove(employee);
            await _db.SaveChangesAsync();
        }

        public async Task<Employee> ChangeEmployeeStatusAsync(int id, string status)
        {
            var validStatuses = new[] { "active", "onleave", "remote", "inactive" };
            if (!validStatuses.Contains(status))
                throw new Exception("Invalid status value");

            var employee = await _db.Employees.FindAsync(id)
                ?? throw new Exception("Employee not found");

            employee.Status = status;
            employee.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return employee;
        }

        public async Task<Employee> UpdateEmployeeProfileAsync(int employeeId, UpdateEmployeeProfileDto dto)
        {
            var employee = await _db.Employees.FindAsync(employeeId)
                ?? throw new Exception("Employee not found");

            if (!string.IsNullOrWhiteSpace(dto.FullName)) employee.FullName = dto.FullName;
            if (!string.IsNullOrWhiteSpace(dto.Phone)) employee.Phone = dto.Phone;
            if (!string.IsNullOrWhiteSpace(dto.Address)) employee.Address = dto.Address;
            if (dto.BirthDate.HasValue) employee.BirthDate = dto.BirthDate.Value;
            if (!string.IsNullOrWhiteSpace(dto.AvatarUrl)) employee.AvatarUrl = dto.AvatarUrl;
            if (!string.IsNullOrWhiteSpace(dto.AvatarPublicId)) employee.AvatarPublicId = dto.AvatarPublicId;

            if (dto.FingerprintFile != null && dto.FingerprintFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await dto.FingerprintFile.CopyToAsync(ms);
                employee.FingerPrint = Convert.ToBase64String(ms.ToArray());
            }
            else if (!string.IsNullOrWhiteSpace(dto.FingerPrint))
            {
                employee.FingerPrint = dto.FingerPrint;
            }

            if (dto.Avatar != null && dto.Avatar.Length > 0)
            {
                using var ms = new MemoryStream();
                await dto.Avatar.CopyToAsync(ms);
                var mimeType = dto.Avatar.ContentType;
                employee.Image = $"data:{mimeType};base64,{Convert.ToBase64String(ms.ToArray())}";
            }
            else if (!string.IsNullOrWhiteSpace(dto.Image))
            {
                employee.Image = dto.Image;
            }

            employee.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return employee;
        }

        public async Task<Employee> UpdateEmployeePasswordAsync(int employeeId, ChangePasswordDto dto)
        {
            if (dto.CurrentPassword == dto.NewPassword)
                throw new Exception("New password must be different from current password");

            if (dto.NewPassword.Length < 6)
                throw new Exception("Password must be at least 6 characters long");

            var employee = await _db.Employees.FindAsync(employeeId)
                ?? throw new Exception("Employee not found");

            if (string.IsNullOrEmpty(employee.PasswordHash) || !BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, employee.PasswordHash))
                throw new Exception("Current password is incorrect");

            employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            employee.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return employee;
        }
        private List<EmployeeDto> Mapemployee(List<Employee> employees)
        {
            List<EmployeeDto> employeeDtos = new List<EmployeeDto>();
            foreach(var employee in employees)
            {
                EmployeeDto employeeDto = new EmployeeDto();
                employeeDto.Id = employee.Id;
                employeeDto.FullName = employee.FullName;
                employeeDto.Phone = employee.Phone;
                employeeDto.Email = employee.Email;
                employeeDto.ImageUrl = employee.Image;
                employeeDto.FingerPrint = employee.FingerPrint;
                employeeDtos.Add(employeeDto);
            }
         
            return employeeDtos;
        }
    }  
}
     public class EmployeeDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string ImageUrl { get; set; }
    public string FingerPrint { get; set; }
}