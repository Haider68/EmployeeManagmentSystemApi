using EmployeeManagmentSystemApi.DTOs.Admin;
using EmployeeManagmentSystemApi.DTOs.Employee;
using EmployeeManagmentSystemApi.Helpers;
using EmployeeManagmentSystemApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagmentSystemApi.Controllers
{
    [Route("api/v1/employee")]
    //[Authorize]
    public class EmployeeController : BaseApiController
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // POST api/v1/employee/create-employee
        // Accepts multipart/form-data (Node.js frontend uses multer for avatar upload)
        [HttpPost("create-employee")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateEmployee([FromForm] CreateEmployeeDto dto)
        {
            try
            {
                var employee = await _employeeService.CreateEmployeeAsync(dto);
                return StatusCode(201, ApiResponse.Success("Employee created successfully", employee));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // GET api/v1/employee/get-all-employees?status=active&department=Engineering
        [HttpGet("get-all-employees")]
        public async Task<IActionResult> GetAllEmployees(
            [FromQuery] string? status,
            [FromQuery] string? department)
        {
            try
            {
                var employees = await _employeeService.GetAllEmployeesAsync(status, department);
                var list = employees.ToList();
                return Ok(new { Success = true, Count = list.Count, Data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error(ex.Message));
            }
        }
        [HttpGet("get-employees")]
        public async Task<IActionResult> GetEmployees()
        {
            try
            {
                var employees = await _employeeService.GetAllEmployeesAsync();
                var list = employees.ToList();

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error(ex.Message));
            }
        }
        // GET api/v1/employee/employee/{id}
        [HttpGet("employee/{id:int}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                return Ok(ApiResponse.Success("Employee retrieved successfully", employee));
            }
            catch (Exception ex)
            {
                return NotFound(ApiResponse.Error(ex.Message));
            }
        }

        // PUT api/v1/employee/update-employee/{id}
        // Accepts multipart/form-data (avatar upload) or application/json
        [HttpPut("update-employee/{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromForm] UpdateEmployeeDto dto)
        {
            try
            {
                var employee = await _employeeService.UpdateEmployeeAsync(id, dto);
                return Ok(ApiResponse.Success("Employee updated successfully", employee));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // DELETE api/v1/employee/delete-employee/{id}
        [HttpDelete("delete-employee/{id:int}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                await _employeeService.DeleteEmployeeAsync(id);
                return Ok(ApiResponse.Success<object>("Employee deleted successfully"));
            }
            catch (Exception ex)
            {
                return NotFound(ApiResponse.Error(ex.Message));
            }
        }

        // PATCH api/v1/employee/{id}/status
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> ChangeEmployeeStatus(
            int id,
            [FromBody] ChangeEmployeeStatusDto dto)
        {
            try
            {
                var employee = await _employeeService.ChangeEmployeeStatusAsync(id, dto.Status);
                return Ok(ApiResponse.Success($"Employee status changed to {dto.Status}", employee));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // PUT api/v1/employee/profile  (employee updates own profile)
        [HttpPut("profile")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateEmployeeProfileDto dto)
        {
            try
            {
                var employeeId = GetCurrentUserId();
                var employee = await _employeeService.UpdateEmployeeProfileAsync(employeeId, dto);
                return Ok(ApiResponse.Success("Profile updated successfully", employee));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // PUT api/v1/employee/update-profile  (alias for /profile)
        [HttpPut("update-profile")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfileAlias([FromForm] UpdateEmployeeProfileDto dto)
            => await UpdateProfile(dto);

        // PUT api/v1/employee/update-password
        [HttpPut("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var employeeId = GetCurrentUserId();
                var employee = await _employeeService.UpdateEmployeePasswordAsync(employeeId, dto);
                return Ok(ApiResponse.Success("Password updated successfully", new
                {
                    Id = employee.Id,
                    FullName = employee.FullName,
                    Email = employee.Email
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }
    }
}
