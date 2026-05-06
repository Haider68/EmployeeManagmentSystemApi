using System.ComponentModel.DataAnnotations;

namespace EmployeeManagmentSystemApi.DTOs.Leave
{
    public class RejectLeaveDto
    {
        [Required]
        public string RejectionReason { get; set; } = string.Empty;
    }
}
