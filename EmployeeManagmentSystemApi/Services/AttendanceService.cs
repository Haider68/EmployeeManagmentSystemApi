using EmployeeManagmentSystemApi.Data;
using EmployeeManagmentSystemApi.DTOs.Attendance;
using EmployeeManagmentSystemApi.Interfaces;
using EmployeeManagmentSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagmentSystemApi.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly AppDbContext _db;

        public AttendanceService(AppDbContext db)
        {
            _db = db;
        }

        private static string TodayString() => DateTime.UtcNow.ToString("yyyy-MM-dd");

        public async Task<Attendance> CheckInAsync(int employeeId)
        {
            var employee = await _db.Employees.FindAsync(employeeId)
                ?? throw new Exception("Employee not found");

            var today = TodayString();
            var existing = await _db.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);

            if (existing != null)
            {
                if (existing.CheckIn.HasValue)
                    return existing; // Already checked in

                existing.CheckIn = DateTime.UtcNow;
                existing.Status = "present";
                existing.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return existing;
            }

            var attendance = new Attendance
            {
                EmployeeId = employeeId,
                Date = today,
                CheckIn = DateTime.UtcNow,
                Status = "present"
            };

            _db.Attendances.Add(attendance);
            await _db.SaveChangesAsync();
            return attendance;
        }

        public async Task<Attendance> CheckOutAsync(int employeeId)
        {
            var today = TodayString();
            var attendance = await _db.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today)
                ?? throw new Exception("No check-in found for today");

            if (attendance.CheckOut.HasValue)
                throw new Exception("Already checked out for today");

            attendance.CheckOut = DateTime.UtcNow;
            attendance.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return attendance;
        }

        public async Task<object> GetTodayAttendanceAsync(int employeeId)
        {
            var today = TodayString();
            var attendance = await _db.Attendances
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);

            if (attendance == null)
                return new { Message = "No attendance record for today", HasCheckedIn = false, HasCheckedOut = false };

            double? workingHoursValue = null;
            if (attendance.CheckIn.HasValue && attendance.CheckOut.HasValue)
                workingHoursValue = (attendance.CheckOut.Value - attendance.CheckIn.Value).TotalHours;

            return new
            {
                Attendance = attendance,
                HasCheckedIn = attendance.CheckIn.HasValue,
                HasCheckedOut = attendance.CheckOut.HasValue,
                WorkingHours = workingHoursValue ?? 0
            };
        }

        public async Task<AttendanceListResult> GetAttendanceByDateRangeAsync(int employeeId, string startDate, string endDate, int page, int limit)
        {
            var skip = (page - 1) * limit;

            var query = _db.Attendances
                .Include(a => a.Employee)
                .Where(a => a.EmployeeId == employeeId && string.Compare(a.Date, startDate) >= 0 && string.Compare(a.Date, endDate) <= 0);

            var total = await query.CountAsync();
            var records = await query
                .OrderByDescending(a => a.Date)
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            return new AttendanceListResult(records, new PaginationInfo(total, page, limit, (int)Math.Ceiling((double)total / limit)));
        }

        public async Task<AttendanceListResult> GetAllAttendanceAsync(int? employeeId, string? status, string? startDate, string? endDate, int page, int limit)
        {
            var skip = (page - 1) * limit;

            var query = _db.Attendances.Include(a => a.Employee).AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(a => a.EmployeeId == employeeId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(a => a.Status == status);

            if (!string.IsNullOrWhiteSpace(startDate))
                query = query.Where(a => string.Compare(a.Date, startDate) >= 0);

            if (!string.IsNullOrWhiteSpace(endDate))
                query = query.Where(a => string.Compare(a.Date, endDate) <= 0);

            var total = await query.CountAsync();
            var records = await query
                .OrderByDescending(a => a.Date)
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            return new AttendanceListResult(records, new PaginationInfo(total, page, limit, (int)Math.Ceiling((double)total / limit)));
        }

        public async Task<Attendance> UpdateAttendanceAsync(int attendanceId, UpdateAttendanceDto dto)
        {
            var attendance = await _db.Attendances.FindAsync(attendanceId)
                ?? throw new Exception("Attendance record not found");

            if (dto.CheckIn.HasValue) attendance.CheckIn = dto.CheckIn.Value;
            if (dto.CheckOut.HasValue) attendance.CheckOut = dto.CheckOut.Value;
            if (!string.IsNullOrWhiteSpace(dto.Status)) attendance.Status = dto.Status;
            attendance.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return attendance;
        }

        public async Task<object> MarkAbsentForDateAsync(string date)
        {
            var activeEmployees = await _db.Employees
                .Where(e => e.Status == "active")
                .Select(e => e.Id)
                .ToListAsync();

            var presentEmployeeIds = await _db.Attendances
                .Where(a => a.Date == date && a.Status == "present")
                .Select(a => a.EmployeeId)
                .ToListAsync();

            var absentEmployeeIds = activeEmployees.Except(presentEmployeeIds).ToList();

            // Avoid duplicates - skip employees who already have a record for this date
            var existingForDate = await _db.Attendances
                .Where(a => a.Date == date && absentEmployeeIds.Contains(a.EmployeeId))
                .Select(a => a.EmployeeId)
                .ToListAsync();

            var newAbsentIds = absentEmployeeIds.Except(existingForDate).ToList();

            var absentRecords = newAbsentIds.Select(empId => new Attendance
            {
                EmployeeId = empId,
                Date = date,
                Status = "absent"
            }).ToList();

            if (absentRecords.Count > 0)
            {
                _db.Attendances.AddRange(absentRecords);
                await _db.SaveChangesAsync();
            }

            return new
            {
                Date = date,
                TotalEmployees = activeEmployees.Count,
                PresentCount = presentEmployeeIds.Count,
                AbsentCount = newAbsentIds.Count,
                AbsentEmployees = newAbsentIds
            };
        }

        public async Task<object> GetAttendanceStatsAsync(int employeeId, int month, int year)
        {
            var startDate = new DateTime(year, month, 1).ToString("yyyy-MM-dd");
            var endDate = new DateTime(year, month, DateTime.DaysInMonth(year, month)).ToString("yyyy-MM-dd");

            var records = await _db.Attendances
                .Where(a => a.EmployeeId == employeeId &&
                            string.Compare(a.Date, startDate) >= 0 &&
                            string.Compare(a.Date, endDate) <= 0)
                .ToListAsync();

            var totalDays = DateTime.DaysInMonth(year, month);
            var presentDays = records.Count(r => r.Status == "present");
            var absentDays = records.Count(r => r.Status == "absent");
            var notMarkedDays = totalDays - presentDays - absentDays;

            long totalMs = 0;
            foreach (var record in records.Where(r => r.Status == "present" && r.CheckIn.HasValue && r.CheckOut.HasValue))
            {
                var diffMs = (long)(record.CheckOut!.Value - record.CheckIn!.Value).TotalMilliseconds;
                if (diffMs > 0) totalMs += diffMs;
            }

            var totalHoursDecimal = totalMs / (1000.0 * 60 * 60);
            var totalMinutes = (int)(totalMs / (1000 * 60));
            var totalHours = totalMinutes / 60;
            var remainingMinutes = totalMinutes % 60;

            var avgPerDay = presentDays > 0 ? totalHoursDecimal / presentDays : 0;
            var avgH = (int)avgPerDay;
            var avgM = (int)((avgPerDay - avgH) * 60);

            return new
            {
                Month = $"{month}/{year}",
                TotalDays = totalDays,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                NotMarkedDays = notMarkedDays,
                TotalWorkingHours = Math.Round(totalHoursDecimal, 2),
                TotalWorkingMinutes = totalMinutes,
                TotalWorkingTimeFormatted = $"{totalHours}h {remainingMinutes}m",
                AverageHoursPerDay = Math.Round(avgPerDay, 2),
                AverageTimeFormatted = $"{avgH}h {avgM}m",
                AttendancePercentage = Math.Round((double)presentDays / totalDays * 100, 2)
            };
        }

        public async Task<object> BulkAttendanceUpdateAsync(BulkAttendanceUpdateDto dto)
        {
            var results = new List<Attendance>();
            var errors = new List<object>();

            foreach (var item in dto.AttendanceData)
            {
                try
                {
                    var dateStr = DateTime.Parse(item.Date).ToString("yyyy-MM-dd");
                    var existing = await _db.Attendances
                        .FirstOrDefaultAsync(a => a.EmployeeId == item.EmployeeId && a.Date == dateStr);

                    if (existing != null)
                    {
                        if (item.CheckIn.HasValue) existing.CheckIn = item.CheckIn.Value;
                        if (item.CheckOut.HasValue) existing.CheckOut = item.CheckOut.Value;
                        if (!string.IsNullOrWhiteSpace(item.Status)) existing.Status = item.Status;
                        existing.UpdatedAt = DateTime.UtcNow;
                        await _db.SaveChangesAsync();
                        results.Add(existing);
                    }
                    else
                    {
                        var record = new Attendance
                        {
                            EmployeeId = item.EmployeeId,
                            Date = dateStr,
                            CheckIn = item.CheckIn,
                            CheckOut = item.CheckOut,
                            Status = item.Status
                        };
                        _db.Attendances.Add(record);
                        await _db.SaveChangesAsync();
                        results.Add(record);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(new { EmployeeId = item.EmployeeId, Date = item.Date, Error = ex.Message });
                }
            }

            return new
            {
                Updated = results.Count,
                Errors = errors.Count,
                Details = new
                {
                    Successful = results.Select(r => r.Id),
                    Errors = errors
                }
            };
        }
    }
}
