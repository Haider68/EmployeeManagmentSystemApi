using System.ComponentModel.DataAnnotations;

namespace EmployeeManagmentSystemApi.DTOs.Project
{
    public class CreateProjectDto
    {
        [Required]
        public string ProjectName { get; set; } = string.Empty;

        [Required]
        public string Client { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        public string? ProjectDescription { get; set; }

        public decimal Budget { get; set; } = 0;

        public string Priority { get; set; } = "medium";

        public List<int> TeamMembers { get; set; } = new();

        public List<string> Tags { get; set; } = new();
    }
}
