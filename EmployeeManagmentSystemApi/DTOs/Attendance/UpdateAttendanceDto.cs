namespace EmployeeManagmentSystemApi.DTOs.Attendance
{
    public class UpdateAttendanceDto
    {
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string? Status { get; set; }
    }
}
