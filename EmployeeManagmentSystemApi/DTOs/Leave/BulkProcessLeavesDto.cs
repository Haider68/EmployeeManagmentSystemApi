using System.ComponentModel.DataAnnotations;

namespace EmployeeManagmentSystemApi.DTOs.Leave
{
    public class BulkProcessLeavesDto
    {
        [Required]
        public List<int> LeaveIds { get; set; } = new();

        [Required]
        public string Action { get; set; } = string.Empty; // approve or reject

        public string? RejectionReason { get; set; }
    }
}
