using EmployeeManagmentSystemApi.DTOs.Project;
using EmployeeManagmentSystemApi.Models;

namespace EmployeeManagmentSystemApi.Interfaces
{
    public record ProjectsResult(IEnumerable<Project> Projects, PaginationInfo Pagination);
    public record PaginationInfo(int Total, int Page, int Limit, int Pages);

    public interface IProjectService
    {
        Task<Project> CreateProjectAsync(CreateProjectDto dto, int createdByEmployeeId);
        Task<ProjectsResult> GetAllProjectsAsync(int page, int limit);
        Task<Project> GetProjectByIdAsync(int id);
        Task<Project> UpdateProjectAsync(int id, UpdateProjectDto dto, int updatedByEmployeeId);
        Task<Project> UpdateProjectStatusAsync(int id, string status);
        Task DeleteProjectAsync(int id);
        Task<object> GetProjectStatsAsync();
        Task<Project> AddTeamMemberAsync(int projectId, int employeeId);
        Task<Project> RemoveTeamMemberAsync(int projectId, int employeeId);
    }
}
