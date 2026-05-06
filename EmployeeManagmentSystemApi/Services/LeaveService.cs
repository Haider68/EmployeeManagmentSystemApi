using EmployeeManagmentSystemApi.Data;
using EmployeeManagmentSystemApi.DTOs.Leave;
using EmployeeManagmentSystemApi.Interfaces;
using EmployeeManagmentSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagmentSystemApi.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly AppDbContext _db;

        public LeaveService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<LeaveRequest> CreateLeaveRequestAsync(int employeeId, CreateLeaveDto dto)
        {
            var employee = await _db.Employees.FindAsync(employeeId)
                ?? throw new Exception("Employee not found");

            // Check for overlapping leave
            var overlap = await _db.LeaveRequests.AnyAsync(lr =>
                lr.EmployeeId == employeeId &&
                (lr.Status == "pending" || lr.Status == "approved") &&
                lr.StartDate <= dto.EndDate && lr.EndDate >= dto.StartDate);

            if (overlap)
                throw new Exception("You already have a pending or approved leave for these dates");

            var days = dto.NumberOfDays ?? (dto.EndDate - dto.StartDate).TotalDays + 1;

            var leave = new LeaveRequest
            {
                EmployeeId = employeeId,
                LeaveType = dto.LeaveType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                NumberOfDays = days,
                Reason = dto.Reason,
                Status = "pending"
            };

            _db.LeaveRequests.Add(leave);
            await _db.SaveChangesAsync();

            return await _db.LeaveRequests
                .Include(lr => lr.Employee)
                .FirstAsync(lr => lr.Id == leave.Id);
        }

        public async Task<LeaveRequest> GetLeaveRequestByIdAsync(int leaveId)
        {
            return await _db.LeaveRequests
                .Include(lr => lr.Employee)
                .Include(lr => lr.ApprovedBy)
                .FirstOrDefaultAsync(lr => lr.Id == leaveId)
                ?? throw new Exception("Leave request not found");
        }

        public async Task<LeaveListResult> GetLeaveRequestsByEmployeeAsync(int employeeId, string? status, string? leaveType, string? startDate, string? endDate, int page, int limit)
        {
            var skip = (page - 1) * limit;

            var query = _db.LeaveRequests
                .Include(lr => lr.Employee)
                .Include(lr => lr.ApprovedBy)
                .Where(lr => lr.EmployeeId == employeeId);

            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(lr => lr.Status == status);
            if (!string.IsNullOrWhiteSpace(leaveType)) query = query.Where(lr => lr.LeaveType == leaveType);
            if (!string.IsNullOrWhiteSpace(startDate)) query = query.Where(lr => lr.StartDate >= DateTime.Parse(startDate));
            if (!string.IsNullOrWhiteSpace(endDate)) query = query.Where(lr => lr.StartDate <= DateTime.Parse(endDate));

            var total = await query.CountAsync();
            var records = await query.OrderByDescending(lr => lr.StartDate).Skip(skip).Take(limit).ToListAsync();

            return new LeaveListResult(records, new PaginationInfo(total, page, limit, (int)Math.Ceiling((double)total / limit)));
        }

        public async Task<LeaveListResult> GetAllLeaveRequestsAsync(string? status, string? leaveType, int? employeeId, string? department, string? startDate, string? endDate, int page, int limit)
        {
            var skip = (page - 1) * limit;

            var query = _db.LeaveRequests
                .Include(lr => lr.Employee)
                .Include(lr => lr.ApprovedBy)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(lr => lr.Status == status);
            if (!string.IsNullOrWhiteSpace(leaveType)) query = query.Where(lr => lr.LeaveType == leaveType);
            if (employeeId.HasValue) query = query.Where(lr => lr.EmployeeId == employeeId.Value);

            if (!string.IsNullOrWhiteSpace(department))
            {
                var deptEmployeeIds = await _db.Employees
                    .Where(e => e.Department == department)
                    .Select(e => e.Id)
                    .ToListAsync();
                query = query.Where(lr => deptEmployeeIds.Contains(lr.EmployeeId));
            }

            if (!string.IsNullOrWhiteSpace(startDate)) query = query.Where(lr => lr.StartDate >= DateTime.Parse(startDate));
            if (!string.IsNullOrWhiteSpace(endDate)) query = query.Where(lr => lr.StartDate <= DateTime.Parse(endDate));

            var total = await query.CountAsync();
            var records = await query.OrderByDescending(lr => lr.CreatedAt).Skip(skip).Take(limit).ToListAsync();

            return new LeaveListResult(records, new PaginationInfo(total, page, limit, (int)Math.Ceiling((double)total / limit)));
        }

        public async Task<LeaveRequest> UpdateLeaveRequestAsync(int leaveId, int employeeId, UpdateLeaveDto dto)
        {
            var leave = await _db.LeaveRequests.FindAsync(leaveId)
                ?? throw new Exception("Leave request not found");

            if (leave.EmployeeId != employeeId)
                throw new Exception("You are not authorized to update this leave request");

            if (leave.Status != "pending")
                throw new Exception("Only pending leave requests can be updated");

            var newStart = dto.StartDate ?? leave.StartDate;
            var newEnd = dto.EndDate ?? leave.EndDate;

            // Check overlaps excluding this request
            var overlap = await _db.LeaveRequests.AnyAsync(lr =>
                lr.Id != leaveId &&
                lr.EmployeeId == employeeId &&
                (lr.Status == "pending" || lr.Status == "approved") &&
                lr.StartDate <= newEnd && lr.EndDate >= newStart);

            if (overlap)
                throw new Exception("You already have a pending or approved leave for these dates");

            if (!string.IsNullOrWhiteSpace(dto.LeaveType)) leave.LeaveType = dto.LeaveType;
            if (dto.StartDate.HasValue) leave.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue) leave.EndDate = dto.EndDate.Value;
            if (!string.IsNullOrWhiteSpace(dto.Reason)) leave.Reason = dto.Reason;

            leave.NumberOfDays = (newEnd - newStart).TotalDays + 1;
            leave.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return await _db.LeaveRequests.Include(lr => lr.Employee).FirstAsync(lr => lr.Id == leaveId);
        }

        public async Task<LeaveRequest> ApproveLeaveRequestAsync(int leaveId, int approvedById)
        {
            var leave = await _db.LeaveRequests.FindAsync(leaveId)
                ?? throw new Exception("Leave request not found");

            if (leave.Status != "pending")
                throw new Exception($"Leave request is already {leave.Status}");

            leave.Status = "approved";
            leave.ApprovedById = approvedById;
            leave.ApprovedAt = DateTime.UtcNow;
            leave.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return await _db.LeaveRequests
                .Include(lr => lr.Employee)
                .Include(lr => lr.ApprovedBy)
                .FirstAsync(lr => lr.Id == leaveId);
        }

        public async Task<LeaveRequest> RejectLeaveRequestAsync(int leaveId, int approvedById, string rejectionReason)
        {
            var leave = await _db.LeaveRequests.FindAsync(leaveId)
                ?? throw new Exception("Leave request not found");

            if (leave.Status != "pending")
                throw new Exception($"Leave request is already {leave.Status}");

            leave.Status = "rejected";
            leave.ApprovedById = approvedById;
            leave.ApprovedAt = DateTime.UtcNow;
            leave.RejectionReason = rejectionReason;
            leave.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return await _db.LeaveRequests
                .Include(lr => lr.Employee)
                .Include(lr => lr.ApprovedBy)
                .FirstAsync(lr => lr.Id == leaveId);
        }

        public async Task<LeaveRequest> CancelLeaveRequestAsync(int leaveId, int employeeId)
        {
            var leave = await _db.LeaveRequests.FindAsync(leaveId)
                ?? throw new Exception("Leave request not found");

            if (leave.EmployeeId != employeeId)
                throw new Exception("You are not authorized to cancel this leave request");

            if (leave.Status != "pending" && leave.Status != "approved")
                throw new Exception($"Cannot cancel a {leave.Status} leave request");

            if (leave.StartDate <= DateTime.UtcNow)
                throw new Exception("Cannot cancel a leave that has already started");

            leave.Status = "cancelled";
            leave.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return await _db.LeaveRequests.Include(lr => lr.Employee).FirstAsync(lr => lr.Id == leaveId);
        }

        public async Task<object> GetLeaveStatisticsAsync(int employeeId, int? year)
        {
            var targetYear = year ?? DateTime.UtcNow.Year;
            var startDate = new DateTime(targetYear, 1, 1);
            var endDate = new DateTime(targetYear, 12, 31, 23, 59, 59);

            var requests = await _db.LeaveRequests
                .Where(lr => lr.EmployeeId == employeeId && lr.StartDate >= startDate && lr.StartDate <= endDate)
                .ToListAsync();

            var byType = new Dictionary<string, object>();
            var byMonth = new int[12];

            int approved = 0, pending = 0, rejected = 0, cancelled = 0;
            double totalDaysTaken = 0;

            foreach (var r in requests)
            {
                switch (r.Status)
                {
                    case "approved": approved++; totalDaysTaken += r.NumberOfDays; break;
                    case "pending": pending++; break;
                    case "rejected": rejected++; break;
                    case "cancelled": cancelled++; break;
                }

                if (!byType.ContainsKey(r.LeaveType))
                    byType[r.LeaveType] = new { Count = 0, Days = 0.0 };

                var existing = (dynamic)byType[r.LeaveType];
                byType[r.LeaveType] = new
                {
                    Count = (int)existing.Count + 1,
                    Days = (double)existing.Days + (r.Status == "approved" ? r.NumberOfDays : 0)
                };

                byMonth[r.StartDate.Month - 1] += (int)r.NumberOfDays;
            }

            return new
            {
                Year = targetYear,
                Statistics = new
                {
                    TotalRequests = requests.Count,
                    Approved = approved,
                    Pending = pending,
                    Rejected = rejected,
                    Cancelled = cancelled,
                    TotalDaysTaken = totalDaysTaken,
                    ByType = byType,
                    ByMonth = byMonth,
                    ApprovalRate = requests.Count > 0
                        ? Math.Round((double)approved / requests.Count * 100, 2)
                        : 0
                }
            };
        }

        public async Task<IEnumerable<LeaveRequest>> GetUpcomingLeavesAsync(string? department, int limit)
        {
            var today = DateTime.UtcNow.Date;

            var query = _db.LeaveRequests
                .Include(lr => lr.Employee)
                .Where(lr => lr.Status == "approved" && lr.StartDate >= today);

            if (!string.IsNullOrWhiteSpace(department))
            {
                var deptEmployeeIds = await _db.Employees
                    .Where(e => e.Department == department)
                    .Select(e => e.Id)
                    .ToListAsync();
                query = query.Where(lr => deptEmployeeIds.Contains(lr.EmployeeId));
            }

            return await query.OrderBy(lr => lr.StartDate).Take(limit).ToListAsync();
        }

        public async Task<object> GetLeaveBalanceAsync(int employeeId)
        {
            var employee = await _db.Employees.FindAsync(employeeId)
                ?? throw new Exception("Employee not found");

            var currentYear = DateTime.UtcNow.Year;
            var startDate = new DateTime(currentYear, 1, 1);
            var endDate = new DateTime(currentYear, 12, 31, 23, 59, 59);

            var approvedLeaves = await _db.LeaveRequests
                .Where(lr => lr.EmployeeId == employeeId && lr.Status == "approved" && lr.StartDate >= startDate && lr.StartDate <= endDate)
                .ToListAsync();

            var totalTaken = approvedLeaves.Sum(l => l.NumberOfDays);

            var entitlement = new Dictionary<string, int>
            {
                { "vacation", 15 },
                { "sick", 10 },
                { "casual", 7 }
            };

            var balance = new Dictionary<string, object>();
            foreach (var (type, allotted) in entitlement)
            {
                var taken = approvedLeaves
                    .Where(l => l.LeaveType == type)
                    .Sum(l => l.NumberOfDays);

                balance[type] = new { Entitlement = allotted, Taken = taken, Balance = allotted - taken };
            }

            return new
            {
                Employee = new
                {
                    Id = employee.Id,
                    Name = employee.FullName,
                    Department = employee.Department
                },
                Year = currentYear,
                TotalTaken = totalTaken,
                Balance = balance
            };
        }

        public async Task<object> BulkProcessLeavesAsync(BulkProcessLeavesDto dto, int approvedById)
        {
            var approved = new List<object>();
            var rejected = new List<object>();
            var errors = new List<object>();

            foreach (var leaveId in dto.LeaveIds)
            {
                try
                {
                    object result;
                    if (dto.Action == "approve")
                    {
                        result = await ApproveLeaveRequestAsync(leaveId, approvedById);
                        approved.Add(new { LeaveId = leaveId, Result = result });
                    }
                    else if (dto.Action == "reject")
                    {
                        result = await RejectLeaveRequestAsync(leaveId, approvedById, dto.RejectionReason ?? string.Empty);
                        rejected.Add(new { LeaveId = leaveId, Result = result });
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(new { LeaveId = leaveId, Error = ex.Message });
                }
            }

            return new
            {
                Approved = approved,
                Rejected = rejected,
                Errors = errors,
                Summary = new
                {
                    Total = dto.LeaveIds.Count,
                    Succeeded = approved.Count + rejected.Count,
                    Failed = errors.Count
                }
            };
        }
    }
}
