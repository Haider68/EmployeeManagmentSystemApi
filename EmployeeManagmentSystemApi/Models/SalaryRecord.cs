using System;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagmentSystemApi.Models
{
    public class SalaryRecord
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        public int Month { get; set; }
        public int Year { get; set; }

        public decimal BaseSalary { get; set; } = 0;
        public decimal TotalWorkedHours { get; set; } = 0;
        public decimal RequiredHours { get; set; } = 160;

        // Deductions
        public decimal ShortageHours { get; set; } = 0;
        public decimal DeductionAmount { get; set; } = 0;

        // Overtime
        public decimal OvertimeHours { get; set; } = 0;
        public decimal OvertimePay { get; set; } = 0;

        // Bonuses
        public decimal BonusesAmount { get; set; } = 0;
        public string? BonusesDetails { get; set; } // e.g. JSON string

        // Final
        public decimal NetSalary { get; set; }
        public string Status { get; set; } = "pending"; // pending, paid

        public DateTime? PaidAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
