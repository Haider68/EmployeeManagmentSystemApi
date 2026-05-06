using EmployeeManagmentSystemApi.DTOs.Attendance;
using EmployeeManagmentSystemApi.Helpers;
using EmployeeManagmentSystemApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagmentSystemApi.Controllers
{
    [Route("api/v1/attendence")]
    //[Authorize]
    public class AttendanceController : BaseApiController
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        // POST api/v1/attendence/check-in
        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn()
        {
            try
            {
                var employeeId = GetCurrentUserId();
                var attendance = await _attendanceService.CheckInAsync(employeeId);
                return Ok(ApiResponse.Success("Check-in recorded successfully", new
                {
                    attendance.Date,
                    attendance.CheckIn,
                    attendance.Status
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // POST api/v1/attendence/check-out
        [HttpPost("check-out")]
        public async Task<IActionResult> CheckOut()
        {
            try
            {
                var employeeId = GetCurrentUserId();
                var attendance = await _attendanceService.CheckOutAsync(employeeId);
                return Ok(ApiResponse.Success("Check-out recorded successfully", new
                {
                    attendance.Date,
                    attendance.CheckIn,
                    attendance.CheckOut
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // GET api/v1/attendence/today/{employeeId}
        [HttpGet("today/{employeeId:int}")]
        public async Task<IActionResult> GetTodayAttendance(int employeeId)
        {
            try
            {
                var result = await _attendanceService.GetTodayAttendanceAsync(employeeId);
                return Ok(ApiResponse.Success("Today's attendance retrieved", result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // GET api/v1/attendence/employee/{employeeId}/range
        [HttpGet("employee/{employeeId:int}/range")]
        public async Task<IActionResult> GetAttendanceByDateRange(
            int employeeId,
            [FromQuery] string startDate,
            [FromQuery] string endDate,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 30)
        {
            try
            {
                var result = await _attendanceService.GetAttendanceByDateRangeAsync(employeeId, startDate, endDate, page, limit);
                return Ok(ApiResponse.Success("Attendance retrieved successfully", result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // GET api/v1/attendence/stats/{employeeId}
        [HttpGet("stats/{employeeId:int}")]
        public async Task<IActionResult> GetAttendanceStats(
            int employeeId,
            [FromQuery] int? month,
            [FromQuery] int? year)
        {
            try
            {
                var now = DateTime.UtcNow;
                var targetMonth = month ?? now.Month;
                var targetYear = year ?? now.Year;
                var stats = await _attendanceService.GetAttendanceStatsAsync(employeeId, targetMonth, targetYear);
                return Ok(ApiResponse.Success("Attendance stats retrieved", stats));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // GET api/v1/attendence/all  (admin)
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAttendance(
            [FromQuery] int? employeeId,
            [FromQuery] string? status,
            [FromQuery] string? startDate,
            [FromQuery] string? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 50)
        {
            try
            {
                var result = await _attendanceService.GetAllAttendanceAsync(employeeId, status, startDate, endDate, page, limit);
                return Ok(ApiResponse.Success("All attendance retrieved", result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // PUT api/v1/attendence/{attendanceId}  (admin update)
        [HttpPut("{attendanceId:int}")]
        public async Task<IActionResult> UpdateAttendance(int attendanceId, [FromBody] UpdateAttendanceDto dto)
        {
            try
            {
                var attendance = await _attendanceService.UpdateAttendanceAsync(attendanceId, dto);
                return Ok(ApiResponse.Success("Attendance updated successfully", attendance));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // POST api/v1/attendence/mark-absent  (admin)
        [HttpPost("mark-absent")]
        public async Task<IActionResult> MarkAbsentForDate()
        {
            try
            {
                var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
                var result = await _attendanceService.MarkAbsentForDateAsync(today);
                return Ok(ApiResponse.Success("Absent employees marked successfully", result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // POST api/v1/attendence/bulk-update  (admin)
        [HttpPost("bulk-update")]
        public async Task<IActionResult> BulkAttendanceUpdate([FromBody] BulkAttendanceUpdateDto dto)
        {
            try
            {
                var result = await _attendanceService.BulkAttendanceUpdateAsync(dto);
                return Ok(ApiResponse.Success("Bulk attendance update completed", result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }
    }
}
