using EmployeeManagmentSystemApi.DTOs.Leave;
using EmployeeManagmentSystemApi.Models;

namespace EmployeeManagmentSystemApi.Interfaces
{
    public record LeaveListResult(IEnumerable<LeaveRequest> LeaveRequests, PaginationInfo Pagination);

    public interface ILeaveService
    {
        Task<LeaveRequest> CreateLeaveRequestAsync(int employeeId, CreateLeaveDto dto);
        Task<LeaveRequest> GetLeaveRequestByIdAsync(int leaveId);
        Task<LeaveListResult> GetLeaveRequestsByEmployeeAsync(int employeeId, string? status, string? leaveType, string? startDate, string? endDate, int page, int limit);
        Task<LeaveListResult> GetAllLeaveRequestsAsync(string? status, string? leaveType, int? employeeId, string? department, string? startDate, string? endDate, int page, int limit);
        Task<LeaveRequest> UpdateLeaveRequestAsync(int leaveId, int employeeId, UpdateLeaveDto dto);
        Task<LeaveRequest> ApproveLeaveRequestAsync(int leaveId, int approvedById);
        Task<LeaveRequest> RejectLeaveRequestAsync(int leaveId, int approvedById, string rejectionReason);
        Task<LeaveRequest> CancelLeaveRequestAsync(int leaveId, int employeeId);
        Task<object> GetLeaveStatisticsAsync(int employeeId, int? year);
        Task<IEnumerable<LeaveRequest>> GetUpcomingLeavesAsync(string? department, int limit);
        Task<object> GetLeaveBalanceAsync(int employeeId);
        Task<object> BulkProcessLeavesAsync(BulkProcessLeavesDto dto, int approvedById);
    }
}
