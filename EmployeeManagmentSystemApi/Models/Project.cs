using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagmentSystemApi.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ProjectName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Client { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        [StringLength(1000)]
        public string? ProjectDescription { get; set; }

        public string Status { get; set; } = "planning"; // planning, in-progress, on-hold, completed, cancelled

        public string Priority { get; set; } = "medium"; // low, medium, high, critical

        public decimal Budget { get; set; } = 0;

        public string? Tags { get; set; } // Can store comma-separated values or JSON array

        public int? CreatedById { get; set; }
        public virtual Employee? CreatedBy { get; set; }

        public int? LastUpdatedById { get; set; }
        public virtual Employee? LastUpdatedBy { get; set; }

        public DateTime? CompletionDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Many-to-many relationship
        public virtual ICollection<Employee> TeamMembers { get; set; } = new List<Employee>();
        public virtual ICollection<ProjectAttachment> Attachments { get; set; } = new List<ProjectAttachment>();
    }

    public class ProjectAttachment
    {
        [Key]
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
