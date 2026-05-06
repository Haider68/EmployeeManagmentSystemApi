using System.ComponentModel.DataAnnotations;

namespace EmployeeManagmentSystemApi.DTOs.Project
{
    public class UpdateProjectStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty; // planning, in-progress, on-hold, completed, cancelled
    }
}
