using System.ComponentModel.DataAnnotations;

namespace EmployeeManagmentSystemApi.DTOs.Salary
{
    public class UpdateSalaryStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty; // pending, paid
    }
}
