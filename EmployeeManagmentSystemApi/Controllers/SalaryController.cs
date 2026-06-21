using EmployeeManagmentSystemApi.DTOs.Salary;
using EmployeeManagmentSystemApi.Helpers;
using EmployeeManagmentSystemApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagmentSystemApi.Controllers
{
    [Route("api/v1/salary")]
   // [Authorize]
    public class SalaryController : BaseApiController
    {
        private readonly ISalaryService _salaryService;

        public SalaryController(ISalaryService salaryService)
        {
            _salaryService = salaryService;
        }

        // POST api/v1/salary/calculate
        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateSalary([FromBody] CalculateSalaryDto dto)
        {
            try
            {
                var record = await _salaryService.CalculateMonthlySalaryAsync(dto.EmployeeId, dto.Month, dto.Year);
                return Ok(ApiResponse.Success("Salary calculated successfully", record));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // POST api/v1/salary/process-all
        [HttpPost("process-all")]
        public async Task<IActionResult> ProcessAllSalaries([FromBody] ProcessAllSalariesDto? dto)
        {
            try
            {
                var now = DateTime.UtcNow;
                var month = dto?.Month ?? now.Month;
                var year = dto?.Year ?? now.Year;

                var records = await _salaryService.ProcessAllSalariesAsync(month, year);
                var list = records.ToList();
                return Ok(ApiResponse.Success($"Processed {list.Count} salaries.", list));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // GET api/v1/salary/records
        [HttpGet("records")]
        public async Task<IActionResult> GetRecords(
            [FromQuery] int? employeeId,
            [FromQuery] int? month,
            [FromQuery] int? year,
            [FromQuery] string? status)
        {
            try
            {
                var records = await _salaryService.GetSalaryRecordsAsync(employeeId, month, year, status);
                return Ok(ApiResponse.Success("Salary records retrieved", records));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // PATCH api/v1/salary/records/{id}/status
        [HttpPatch("records/{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateSalaryStatusDto dto)
        {
            try
            {
                var record = await _salaryService.UpdateSalaryStatusAsync(id, dto.Status);
                return Ok(ApiResponse.Success("Status updated successfully", record));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }
    }
}
