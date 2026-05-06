using System;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagmentSystemApi.Models
{
    public class LeaveRequest
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        [Required]
        public string LeaveType { get; set; } = string.Empty; // vacation, sick, personal, etc.

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public double NumberOfDays { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        public string Status { get; set; } = "pending"; // pending, approved, rejected, cancelled

        public int? ApprovedById { get; set; }
        public virtual Employee? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
