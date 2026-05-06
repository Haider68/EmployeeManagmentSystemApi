using System;
using System.Threading.Tasks;
using EmployeeManagmentSystemApi.Data;
using EmployeeManagmentSystemApi.Interfaces;
using EmployeeManagmentSystemApi.Models;

namespace EmployeeManagmentSystemApi.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IGenericRepository<Employee> Employees { get; private set; }
        public IGenericRepository<Admin> Admins { get; private set; }
        public IGenericRepository<Project> Projects { get; private set; }
        public IGenericRepository<ProjectAttachment> ProjectAttachments { get; private set; }
        public IGenericRepository<LeaveRequest> LeaveRequests { get; private set; }
        public IGenericRepository<Attendance> Attendances { get; private set; }
        public IGenericRepository<SalaryRecord> SalaryRecords { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Employees = new GenericRepository<Employee>(_context);
            Admins = new GenericRepository<Admin>(_context);
            Projects = new GenericRepository<Project>(_context);
            ProjectAttachments = new GenericRepository<ProjectAttachment>(_context);
            LeaveRequests = new GenericRepository<LeaveRequest>(_context);
            Attendances = new GenericRepository<Attendance>(_context);
            SalaryRecords = new GenericRepository<SalaryRecord>(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
