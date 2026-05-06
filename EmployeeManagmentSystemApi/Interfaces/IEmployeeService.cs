using EmployeeManagmentSystemApi.DTOs.Admin;
using EmployeeManagmentSystemApi.DTOs.Employee;
using EmployeeManagmentSystemApi.Models;

namespace EmployeeManagmentSystemApi.Interfaces
{
    public interface IEmployeeService
    {
        Task<Employee> CreateEmployeeAsync(CreateEmployeeDto dto);
        Task<IEnumerable<Employee>> GetAllEmployeesAsync(string? status, string? department);
        Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
        Task<Employee> GetEmployeeByIdAsync(int id);
        Task<Employee> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto);
        Task DeleteEmployeeAsync(int id);
        Task<Employee> ChangeEmployeeStatusAsync(int id, string status);
        Task<Employee> UpdateEmployeeProfileAsync(int employeeId, UpdateEmployeeProfileDto dto);
        Task<Employee> UpdateEmployeePasswordAsync(int employeeId, ChangePasswordDto dto);
    }
}
