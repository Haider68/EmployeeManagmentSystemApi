using System.ComponentModel.DataAnnotations;

namespace EmployeeManagmentSystemApi.DTOs.Employee
{
    /// <summary>
    /// Used by POST /create-employee. Supports both application/json and multipart/form-data
    /// (the Node.js frontend sends multipart because it also uploads an optional avatar file).
    /// </summary>
    public class CreateEmployeeDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Position { get; set; } = string.Empty;

        public string Department { get; set; } = "Engineering";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "active";

        public DateTime? BirthDate { get; set; }

        public string? Address { get; set; }

        public string Role { get; set; } = "employee";

        public decimal Salary { get; set; } = 0;
        public decimal HourlyRate { get; set; } = 0;
        public int RequiredHoursPerMonth { get; set; } = 160;
        public decimal OvertimeRate { get; set; } = 0;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        // Employee photo — URL or base64 string (from enrollment system)
        public string? Image { get; set; }

        // Fingerprint data — base64-encoded fingerprint template from sensor
        public string? FingerPrint { get; set; }

        // Avatar file — optional, accepted from multipart/form-data uploads
        public IFormFile? Avatar { get; set; }

        // Fingerprint image file — optional, from multipart/form-data uploads
        public IFormFile? FingerprintFile { get; set; }
    }
}
