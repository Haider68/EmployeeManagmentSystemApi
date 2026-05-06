using System;
using System.Threading.Tasks;

namespace EmployeeManagmentSystemApi.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Models.Employee> Employees { get; }
        IGenericRepository<Models.Admin> Admins { get; }
        IGenericRepository<Models.Project> Projects { get; }
        IGenericRepository<Models.ProjectAttachment> ProjectAttachments { get; }
        IGenericRepository<Models.LeaveRequest> LeaveRequests { get; }
        IGenericRepository<Models.Attendance> Attendances { get; }
        IGenericRepository<Models.SalaryRecord> SalaryRecords { get; }

        Task<int> CompleteAsync();
    }
}
