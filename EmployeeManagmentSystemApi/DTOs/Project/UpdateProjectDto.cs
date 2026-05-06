namespace EmployeeManagmentSystemApi.DTOs.Project
{
    public class UpdateProjectDto
    {
        public string? ProjectName { get; set; }
        public string? Client { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? Deadline { get; set; }
        public string? ProjectDescription { get; set; }
        public decimal? Budget { get; set; }
        public string? Priority { get; set; }
        public List<int>? TeamMembers { get; set; }
        public List<string>? Tags { get; set; }
    }
}
