namespace EmployeeManagmentSystemApi.DTOs.Attendance
{
    public class BulkAttendanceUpdateDto
    {
        public List<BulkAttendanceItemDto> AttendanceData { get; set; } = new();
    }

    public class BulkAttendanceItemDto
    {
        public int EmployeeId { get; set; }
        public string Date { get; set; } = string.Empty;
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string Status { get; set; } = "not-marked";
    }
}
