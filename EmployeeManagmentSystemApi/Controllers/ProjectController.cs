using EmployeeManagmentSystemApi.DTOs.Project;
using EmployeeManagmentSystemApi.Helpers;
using EmployeeManagmentSystemApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagmentSystemApi.Controllers
{
    [Route("api/v1/project")]
    [Authorize]
    public class ProjectController : BaseApiController
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        // POST api/v1/project/create-project
        [HttpPost("create-project")]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
        {
            try
            {
                // If user is employee, pass their ID as creator; if admin, pass 0 (no FK)
                var userType = GetCurrentUserType();
                var creatorId = userType == "employee" ? GetCurrentUserId() : 0;

                // For admin, we use 0 which will fail FK unless we handle it
                // Use the first available employee ID as a fallback for admin creators
                var project = await _projectService.CreateProjectAsync(dto, creatorId);
                return StatusCode(201, ApiResponse.Success("Project created successfully", project));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error(ex.Message));
            }
        }

        // GET api/v1/project/get-all-projects
        [HttpGet("get-all-projects")]
        public async Task<IActionResult> GetAllProjects([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            try
            {
                var result = await _projectService.GetAllProjectsAsync(page, limit);
                return Ok(new { Success = true, Data = result.Projects, Pagination = result.Pagination });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error(ex.Message));
            }
        }

        // GET api/v1/project/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetProjectStats()
        {
            try
            {
                var stats = await _projectService.GetProjectStatsAsync();
                return Ok(ApiResponse.Success("Stats retrieved successfully", stats));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error(ex.Message));
            }
        }

        // GET api/v1/project/get-project/{id}
        [HttpGet("get-project/{id:int}")]
        public async Task<IActionResult> GetProject(int id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                return Ok(ApiResponse.Success("Project retrieved successfully", project));
            }
            catch (Exception ex)
            {
                return NotFound(ApiResponse.Error(ex.Message));
            }
        }

        // PUT api/v1/project/update-project/{id}
        [HttpPut("update-project/{id:int}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectDto dto)
        {
            try
            {
                var userType = GetCurrentUserType();
                var updaterId = userType == "employee" ? GetCurrentUserId() : 0;
                var project = await _projectService.UpdateProjectAsync(id, dto, updaterId);
                return Ok(ApiResponse.Success("Project updated successfully", project));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error(ex.Message));
            }
        }

        // PATCH api/v1/project/update-status/{id}/status
        [HttpPatch("update-status/{id:int}/status")]
        public async Task<IActionResult> UpdateProjectStatus(int id, [FromBody] UpdateProjectStatusDto dto)
        {
            try
            {
                var project = await _projectService.UpdateProjectStatusAsync(id, dto.Status);
                return Ok(ApiResponse.Success("Project status updated successfully", project));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error(ex.Message));
            }
        }

        // DELETE api/v1/project/delete-project/{id}
        [HttpDelete("delete-project/{id:int}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                await _projectService.DeleteProjectAsync(id);
                return Ok(ApiResponse.Success<object>("Project deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error(ex.Message));
            }
        }

        // POST api/v1/project/{id}/team-members
        [HttpPost("{id:int}/team-members")]
        public async Task<IActionResult> AddTeamMember(int id, [FromBody] AddTeamMemberDto dto)
        {
            try
            {
                var project = await _projectService.AddTeamMemberAsync(id, dto.EmployeeId);
                return Ok(ApiResponse.Success("Team member added successfully", project));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error(ex.Message));
            }
        }

        // DELETE api/v1/project/{id}/team-members/{employeeId}
        [HttpDelete("{id:int}/team-members/{employeeId:int}")]
        public async Task<IActionResult> RemoveTeamMember(int id, int employeeId)
        {
            try
            {
                var project = await _projectService.RemoveTeamMemberAsync(id, employeeId);
                return Ok(ApiResponse.Success("Team member removed successfully", project));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error(ex.Message));
            }
        }
    }
}
