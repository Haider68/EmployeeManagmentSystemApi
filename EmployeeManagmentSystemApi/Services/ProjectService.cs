using EmployeeManagmentSystemApi.Data;
using EmployeeManagmentSystemApi.DTOs.Project;
using EmployeeManagmentSystemApi.Interfaces;
using EmployeeManagmentSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagmentSystemApi.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _db;

        public ProjectService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Project> CreateProjectAsync(CreateProjectDto dto, int createdByEmployeeId)
        {
            // Validate team members are active employees
            if (dto.TeamMembers.Count > 0)
            {
                var activeEmployees = await _db.Employees
                    .Where(e => dto.TeamMembers.Contains(e.Id) && e.Status == "active")
                    .CountAsync();

                if (activeEmployees != dto.TeamMembers.Count)
                    throw new Exception("One or more team members are invalid or inactive");
            }

            var project = new Project
            {
                ProjectName = dto.ProjectName,
                Client = dto.Client,
                StartDate = dto.StartDate,
                Deadline = dto.Deadline,
                ProjectDescription = dto.ProjectDescription,
                Budget = dto.Budget,
                Priority = dto.Priority,
                Tags = dto.Tags.Count > 0 ? string.Join(",", dto.Tags) : null,
                CreatedById = createdByEmployeeId,
                LastUpdatedById = createdByEmployeeId
            };

            // Load team members
            if (dto.TeamMembers.Count > 0)
            {
                var members = await _db.Employees
                    .Where(e => dto.TeamMembers.Contains(e.Id))
                    .ToListAsync();
                project.TeamMembers = members;
            }

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();
            return project;
        }

        public async Task<ProjectsResult> GetAllProjectsAsync(int page, int limit)
        {
            var skip = (page - 1) * limit;

            var projects = await _db.Projects
                .Include(p => p.TeamMembers)
                .Include(p => p.CreatedBy)
                .Include(p => p.LastUpdatedBy)
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            var total = await _db.Projects.CountAsync();
            var pagination = new PaginationInfo(total, page, limit, (int)Math.Ceiling((double)total / limit));

            return new ProjectsResult(projects, pagination);
        }

        public async Task<Project> GetProjectByIdAsync(int id)
        {
            return await _db.Projects
                .Include(p => p.TeamMembers)
                .Include(p => p.CreatedBy)
                .Include(p => p.LastUpdatedBy)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new Exception("Project not found");
        }

        public async Task<Project> UpdateProjectAsync(int id, UpdateProjectDto dto, int updatedByEmployeeId)
        {
            var project = await _db.Projects
                .Include(p => p.TeamMembers)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new Exception("Project not found");

            if (!string.IsNullOrWhiteSpace(dto.ProjectName)) project.ProjectName = dto.ProjectName;
            if (!string.IsNullOrWhiteSpace(dto.Client)) project.Client = dto.Client;
            if (dto.StartDate.HasValue) project.StartDate = dto.StartDate.Value;
            if (dto.Deadline.HasValue) project.Deadline = dto.Deadline.Value;
            if (dto.ProjectDescription != null) project.ProjectDescription = dto.ProjectDescription;
            if (dto.Budget.HasValue) project.Budget = dto.Budget.Value;
            if (!string.IsNullOrWhiteSpace(dto.Priority)) project.Priority = dto.Priority;
            if (dto.Tags != null) project.Tags = string.Join(",", dto.Tags);

            if (dto.TeamMembers != null)
            {
                if (dto.TeamMembers.Count > 0)
                {
                    var activeCount = await _db.Employees
                        .Where(e => dto.TeamMembers.Contains(e.Id) && e.Status == "active")
                        .CountAsync();
                    if (activeCount != dto.TeamMembers.Count)
                        throw new Exception("One or more team members are invalid or inactive");
                }

                var members = await _db.Employees
                    .Where(e => dto.TeamMembers.Contains(e.Id))
                    .ToListAsync();
                project.TeamMembers = members;
            }

            project.LastUpdatedById = updatedByEmployeeId;
            project.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return project;
        }

        public async Task<Project> UpdateProjectStatusAsync(int id, string status)
        {
            var validStatuses = new[] { "planning", "in-progress", "on-hold", "completed", "cancelled" };
            if (!validStatuses.Contains(status))
                throw new Exception("Invalid status value");

            var project = await _db.Projects.FindAsync(id)
                ?? throw new Exception("Project not found");

            project.Status = status;
            if (status == "completed") project.CompletionDate = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return project;
        }

        public async Task DeleteProjectAsync(int id)
        {
            var project = await _db.Projects.FindAsync(id)
                ?? throw new Exception("Project not found");

            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();
        }

        public async Task<object> GetProjectStatsAsync()
        {
            var total = await _db.Projects.CountAsync();
            var planning = await _db.Projects.CountAsync(p => p.Status == "planning");
            var inProgress = await _db.Projects.CountAsync(p => p.Status == "in-progress");
            var completed = await _db.Projects.CountAsync(p => p.Status == "completed");
            var overdue = await _db.Projects.CountAsync(p =>
                p.Deadline < DateTime.UtcNow &&
                (p.Status == "planning" || p.Status == "in-progress"));

            var budgetStats = await _db.Projects
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalBudget = g.Sum(p => p.Budget),
                    AverageBudget = g.Average(p => p.Budget)
                })
                .FirstOrDefaultAsync();

            return new
            {
                TotalProjects = total,
                Planning = planning,
                InProgress = inProgress,
                Completed = completed,
                Overdue = overdue,
                TotalBudget = budgetStats?.TotalBudget ?? 0,
                AverageBudget = budgetStats?.AverageBudget ?? 0
            };
        }

        public async Task<Project> AddTeamMemberAsync(int projectId, int employeeId)
        {
            var project = await _db.Projects
                .Include(p => p.TeamMembers)
                .FirstOrDefaultAsync(p => p.Id == projectId)
                ?? throw new Exception("Project not found");

            var employee = await _db.Employees.FindAsync(employeeId)
                ?? throw new Exception("Employee not found or inactive");

            if (employee.Status != "active")
                throw new Exception("Employee not found or inactive");

            if (!project.TeamMembers.Any(m => m.Id == employeeId))
            {
                project.TeamMembers.Add(employee);
                await _db.SaveChangesAsync();
            }

            return project;
        }

        public async Task<Project> RemoveTeamMemberAsync(int projectId, int employeeId)
        {
            var project = await _db.Projects
                .Include(p => p.TeamMembers)
                .FirstOrDefaultAsync(p => p.Id == projectId)
                ?? throw new Exception("Project not found");

            var member = project.TeamMembers.FirstOrDefault(m => m.Id == employeeId);
            if (member != null)
            {
                project.TeamMembers.Remove(member);
                await _db.SaveChangesAsync();
            }

            return project;
        }
    }
}
