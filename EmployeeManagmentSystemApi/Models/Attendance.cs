using System;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagmentSystemApi.Models
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Date { get; set; } = string.Empty; // Store as string like "yyyy-MM-dd"

        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }

        public string Status { get; set; } = "not-marked"; // present, absent, not-marked

        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
