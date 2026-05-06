using EmployeeManagmentSystemApi.DTOs.Attendance;
using EmployeeManagmentSystemApi.Models;

namespace EmployeeManagmentSystemApi.Interfaces
{
    public record AttendanceListResult(IEnumerable<Attendance> AttendanceRecords, PaginationInfo Pagination);

    public interface IAttendanceService
    {
        Task<Attendance> CheckInAsync(int employeeId);
        Task<Attendance> CheckOutAsync(int employeeId);
        Task<object> GetTodayAttendanceAsync(int employeeId);
        Task<AttendanceListResult> GetAttendanceByDateRangeAsync(int employeeId, string startDate, string endDate, int page, int limit);
        Task<AttendanceListResult> GetAllAttendanceAsync(int? employeeId, string? status, string? startDate, string? endDate, int page, int limit);
        Task<Attendance> UpdateAttendanceAsync(int attendanceId, UpdateAttendanceDto dto);
        Task<object> MarkAbsentForDateAsync(string date);
        Task<object> GetAttendanceStatsAsync(int employeeId, int month, int year);
        Task<object> BulkAttendanceUpdateAsync(BulkAttendanceUpdateDto dto);
    }
}
