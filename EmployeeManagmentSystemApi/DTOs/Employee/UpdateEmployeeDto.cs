namespace EmployeeManagmentSystemApi.DTOs.Employee
{
    public class UpdateEmployeeDto
    {
        public string? FullName { get; set; }
        public string? Position { get; set; }
        public string? Department { get; set; }
        public string? Phone { get; set; }
        public DateTime? JoinDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public decimal? Salary { get; set; }
        public decimal? HourlyRate { get; set; }
        public int? RequiredHoursPerMonth { get; set; }
        public decimal? OvertimeRate { get; set; }
        public string? AvatarUrl { get; set; }
        public string? AvatarPublicId { get; set; }

        // Employee photo — URL or base64 string
        public string? Image { get; set; }

        // Fingerprint data — base64-encoded fingerprint template
        public string? FingerPrint { get; set; }

        // Avatar file — optional, accepted from multipart/form-data uploads
        public IFormFile? Avatar { get; set; }

        // Fingerprint image file — optional
        public IFormFile? FingerprintFile { get; set; }
    }
}
