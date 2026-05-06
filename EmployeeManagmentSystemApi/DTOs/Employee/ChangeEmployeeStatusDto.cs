using System.ComponentModel.DataAnnotations;

namespace EmployeeManagmentSystemApi.DTOs.Employee
{
    public class ChangeEmployeeStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty; // active, onleave, remote, inactive
    }
}
