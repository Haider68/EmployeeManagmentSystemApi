namespace EmployeeManagmentSystemApi.DTOs.Leave
{
    public class UpdateLeaveDto
    {
        public string? LeaveType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Reason { get; set; }
    }
}
