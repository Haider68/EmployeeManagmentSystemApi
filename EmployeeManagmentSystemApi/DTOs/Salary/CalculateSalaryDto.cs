using System.ComponentModel.DataAnnotations;

namespace EmployeeManagmentSystemApi.DTOs.Salary
{
    public class CalculateSalaryDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }
    }
}
