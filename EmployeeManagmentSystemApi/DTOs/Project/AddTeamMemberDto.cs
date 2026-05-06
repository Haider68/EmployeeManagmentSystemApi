using System.ComponentModel.DataAnnotations;

namespace EmployeeManagmentSystemApi.DTOs.Project
{
    public class AddTeamMemberDto
    {
        [Required]
        public int EmployeeId { get; set; }
    }
}
