using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagmentSystemApi.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Position { get; set; } = string.Empty;

        public string Department { get; set; } = "Engineering";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public DateTime JoinDate { get; set; }
        public DateTime? ValidUntil { get; set; }

        public string Status { get; set; } = "active"; // active, onleave, remote, inactive

        public DateTime? BirthDate { get; set; }

        public string? Address { get; set; }

        public string Role { get; set; } = "employee";

        public string? AvatarUrl { get; set; }
        public string? AvatarPublicId { get; set; }

        // Employee photo captured from the enrollment / HR system (stored as URL or base64)
        public string? Image { get; set; }

        // Fingerprint data stored as base64-encoded string (template from fingerprint sensor)
        public string? FingerPrint { get; set; }

        public decimal Salary { get; set; } = 0;
        public decimal HourlyRate { get; set; } = 0;
        public int RequiredHoursPerMonth { get; set; } = 160;
        public decimal OvertimeRate { get; set; } = 0;

        public string? PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
        public virtual ICollection<Project>? CreatedProjects { get; set; }
        public virtual ICollection<Project>? UpdatedProjects { get; set; }
        public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public virtual ICollection<LeaveRequest> ApprovedLeaveRequests { get; set; } = new List<LeaveRequest>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<SalaryRecord> SalaryRecords { get; set; } = new List<SalaryRecord>();
    }
}
