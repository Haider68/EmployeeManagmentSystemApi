using System.ComponentModel.DataAnnotations;

namespace EmployeeManagmentSystemApi.DTOs.Leave
{
    public class CreateLeaveDto
    {
        [Required]
        public string LeaveType { get; set; } = string.Empty; // vacation, sick, personal, maternity, paternity, bereavement, casual, compensatory

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        public double? NumberOfDays { get; set; }
    }
}
