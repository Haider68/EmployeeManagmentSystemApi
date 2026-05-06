using EmployeeManagmentSystemApi.Models;

namespace EmployeeManagmentSystemApi.Interfaces
{
    public interface ISalaryService
    {
        Task<SalaryRecord> CalculateMonthlySalaryAsync(int employeeId, int month, int year);
        Task<IEnumerable<SalaryRecord>> ProcessAllSalariesAsync(int month, int year);
        Task<IEnumerable<SalaryRecord>> GetSalaryRecordsAsync(int? employeeId, int? month, int? year, string? status);
        Task<SalaryRecord> UpdateSalaryStatusAsync(int recordId, string status);
    }
}
