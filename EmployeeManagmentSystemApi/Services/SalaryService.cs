using EmployeeManagmentSystemApi.Data;
using EmployeeManagmentSystemApi.Interfaces;
using EmployeeManagmentSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagmentSystemApi.Services
{
    public class SalaryService : ISalaryService
    {
        private readonly AppDbContext _db;

        public SalaryService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<SalaryRecord> CalculateMonthlySalaryAsync(int employeeId, int month, int year)
        {
            var employee = await _db.Employees.FindAsync(employeeId)
                ?? throw new Exception("Employee not found");

            var startDate = new DateTime(year, month, 1).ToString("yyyy-MM-dd");
            var endDate = new DateTime(year, month, DateTime.DaysInMonth(year, month)).ToString("yyyy-MM-dd");

            var attendances = await _db.Attendances
                .Where(a => a.EmployeeId == employeeId &&
                            a.Status == "present" &&
                            string.Compare(a.Date, startDate) >= 0 &&
                            string.Compare(a.Date, endDate) <= 0)
                .ToListAsync();

            double totalWorkedHours = 0;
            foreach (var att in attendances.Where(r => r.CheckIn.HasValue && r.CheckOut.HasValue))
            {
                var hours = (att.CheckOut!.Value - att.CheckIn!.Value).TotalHours;
                if (hours > 0) totalWorkedHours += hours;
            }

            totalWorkedHours = Math.Round(totalWorkedHours, 2);

            var requiredHours = (double)employee.RequiredHoursPerMonth;
            var baseSalary = (double)employee.Salary;
            var hourlyRate = employee.HourlyRate > 0
                ? (double)employee.HourlyRate
                : (requiredHours > 0 ? baseSalary / requiredHours : 0);
            var overtimeRate = employee.OvertimeRate > 0
                ? (double)employee.OvertimeRate
                : hourlyRate * 1.5;

            double overtimeHours = 0, shortageHours = 0, overtimePay = 0, deductionAmount = 0;

            if (totalWorkedHours > requiredHours)
            {
                overtimeHours = totalWorkedHours - requiredHours;
                overtimePay = overtimeHours * overtimeRate;
            }
            else if (totalWorkedHours < requiredHours)
            {
                shortageHours = requiredHours - totalWorkedHours;
                deductionAmount = shortageHours * hourlyRate;
            }

            var netSalary = baseSalary + overtimePay - deductionAmount;

            var existing = await _db.SalaryRecords
                .FirstOrDefaultAsync(sr => sr.EmployeeId == employeeId && sr.Month == month && sr.Year == year);

            if (existing != null)
            {
                existing.TotalWorkedHours = (decimal)totalWorkedHours;
                existing.RequiredHours = (decimal)requiredHours;
                existing.ShortageHours = (decimal)shortageHours;
                existing.DeductionAmount = (decimal)deductionAmount;
                existing.OvertimeHours = (decimal)overtimeHours;
                existing.OvertimePay = (decimal)overtimePay;
                existing.NetSalary = (decimal)netSalary;
                existing.BaseSalary = (decimal)baseSalary;
                existing.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return existing;
            }

            var record = new SalaryRecord
            {
                EmployeeId = employeeId,
                Month = month,
                Year = year,
                BaseSalary = (decimal)baseSalary,
                TotalWorkedHours = (decimal)totalWorkedHours,
                RequiredHours = (decimal)requiredHours,
                ShortageHours = (decimal)shortageHours,
                DeductionAmount = (decimal)deductionAmount,
                OvertimeHours = (decimal)overtimeHours,
                OvertimePay = (decimal)overtimePay,
                BonusesAmount = 0,
                NetSalary = (decimal)netSalary,
                Status = "pending"
            };

            _db.SalaryRecords.Add(record);
            await _db.SaveChangesAsync();
            return record;
        }

        public async Task<IEnumerable<SalaryRecord>> ProcessAllSalariesAsync(int month, int year)
        {
            var activeEmployees = await _db.Employees
                .Where(e => e.Status == "active")
                .ToListAsync();

            var results = new List<SalaryRecord>();
            foreach (var emp in activeEmployees)
            {
                try
                {
                    var salaryRecord = await CalculateMonthlySalaryAsync(emp.Id, month, year);
                    results.Add(salaryRecord);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error processing salary for employee {emp.Id}: {ex.Message}");
                }
            }

            return results;
        }

        public async Task<IEnumerable<SalaryRecord>> GetSalaryRecordsAsync(int? employeeId, int? month, int? year, string? status)
        {
            var query = _db.SalaryRecords.Include(sr => sr.Employee).AsQueryable();

            if (employeeId.HasValue) query = query.Where(sr => sr.EmployeeId == employeeId.Value);
            if (month.HasValue) query = query.Where(sr => sr.Month == month.Value);
            if (year.HasValue) query = query.Where(sr => sr.Year == year.Value);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(sr => sr.Status == status);

            return await query.OrderByDescending(sr => sr.Year).ThenByDescending(sr => sr.Month).ToListAsync();
        }

        public async Task<SalaryRecord> UpdateSalaryStatusAsync(int recordId, string status)
        {
            var record = await _db.SalaryRecords.FindAsync(recordId)
                ?? throw new Exception("Salary record not found");

            record.Status = status;
            if (status == "paid") record.PaidAt = DateTime.UtcNow;
            record.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return record;
        }
    }
}
