using EmployeeManagmentSystemApi.DTOs.Leave;
using EmployeeManagmentSystemApi.Helpers;
using EmployeeManagmentSystemApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagmentSystemApi.Controllers
{
    [Route("api/v1/leave")]
    [Authorize]
    public class LeaveController : BaseApiController
    {
        private readonly ILeaveService _leaveService;

        public LeaveController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        // POST api/v1/leave/create-leave
        [HttpPost("create-leave")]
        public async Task<IActionResult> CreateLeaveRequest([FromBody] CreateLeaveDto dto)
        {
            try
            {
                var employeeId = GetCurrentUserId();
                var leave = await _leaveService.CreateLeaveRequestAsync(employeeId, dto);
                return StatusCode(201, ApiResponse.Success("Leave request submitted successfully", leave));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // GET api/v1/leave/my-leaves
        [HttpGet("my-leaves")]
        public async Task<IActionResult> GetEmployeeLeaveRequests(
            [FromQuery] string? status,
            [FromQuery] string? leave_type,
            [FromQuery] string? start_date,
            [FromQuery] string? end_date,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20)
        {
            try
            {
                var employeeId = GetCurrentUserId();
                var result = await _leaveService.GetLeaveRequestsByEmployeeAsync(employeeId, status, leave_type, start_date, end_date, page, limit);
                return Ok(ApiResponse.Success("Leave requests retrieved", result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // GET api/v1/leave/balance
        [HttpGet("balance")]
        public async Task<IActionResult> GetLeaveBalance()
        {
            try
            {
                var employeeId = GetCurrentUserId();
                var balance = await _leaveService.GetLeaveBalanceAsync(employeeId);
                return Ok(ApiResponse.Success("Leave balance retrieved", balance));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // GET api/v1/leave/statistics
        [HttpGet("statistics")]
        public async Task<IActionResult> GetLeaveStatistics([FromQuery] int? year)
        {
            try
            {
                var employeeId = GetCurrentUserId();
                var stats = await _leaveService.GetLeaveStatisticsAsync(employeeId, year);
                return Ok(ApiResponse.Success("Leave statistics retrieved", stats));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // GET api/v1/leave/all  (admin)
        [HttpGet("all")]
        public async Task<IActionResult> GetAllLeaveRequests(
            [FromQuery] string? status,
            [FromQuery] string? leave_type,
            [FromQuery] int? employee_id,
            [FromQuery] string? department,
            [FromQuery] string? start_date,
            [FromQuery] string? end_date,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 50)
        {
            try
            {
                var result = await _leaveService.GetAllLeaveRequestsAsync(status, leave_type, employee_id, department, start_date, end_date, page, limit);
                return Ok(ApiResponse.Success("All leave requests retrieved", result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // GET api/v1/leave/upcoming  (admin)
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingLeaves(
            [FromQuery] string? department,
            [FromQuery] int limit = 10)
        {
            try
            {
                var leaves = await _leaveService.GetUpcomingLeavesAsync(department, limit);
                return Ok(ApiResponse.Success("Upcoming leaves retrieved", leaves));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // POST api/v1/leave/bulk-process
        [HttpPost("bulk-process")]
        public async Task<IActionResult> BulkProcessLeaves([FromBody] BulkProcessLeavesDto dto)
        {
            try
            {
                var approvedById = GetCurrentUserId();
                var result = await _leaveService.BulkProcessLeavesAsync(dto, approvedById);
                return Ok(ApiResponse.Success($"Bulk {dto.Action} completed", result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // GET api/v1/leave/{leaveId}
        [HttpGet("{leaveId:int}")]
        public async Task<IActionResult> GetLeaveRequest(int leaveId)
        {
            try
            {
                var leave = await _leaveService.GetLeaveRequestByIdAsync(leaveId);
                return Ok(ApiResponse.Success("Leave request retrieved", leave));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // PUT api/v1/leave/{leaveId}
        [HttpPut("{leaveId:int}")]
        public async Task<IActionResult> UpdateLeaveRequest(int leaveId, [FromBody] UpdateLeaveDto dto)
        {
            try
            {
                var employeeId = GetCurrentUserId();
                var leave = await _leaveService.UpdateLeaveRequestAsync(leaveId, employeeId, dto);
                return Ok(ApiResponse.Success("Leave request updated successfully", leave));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // PUT api/v1/leave/{leaveId}/cancel
        [HttpPut("{leaveId:int}/cancel")]
        public async Task<IActionResult> CancelLeaveRequest(int leaveId)
        {
            try
            {
                var employeeId = GetCurrentUserId();
                var leave = await _leaveService.CancelLeaveRequestAsync(leaveId, employeeId);
                return Ok(ApiResponse.Success("Leave request cancelled successfully", leave));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // PUT api/v1/leave/{leaveId}/approve
        [HttpPut("{leaveId:int}/approve")]
        public async Task<IActionResult> ApproveLeaveRequest(int leaveId)
        {
            try
            {
                var approvedById = GetCurrentUserId();
                var leave = await _leaveService.ApproveLeaveRequestAsync(leaveId, approvedById);
                return Ok(ApiResponse.Success("Leave request approved successfully", leave));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // PUT api/v1/leave/{leaveId}/reject
        [HttpPut("{leaveId:int}/reject")]
        public async Task<IActionResult> RejectLeaveRequest(int leaveId, [FromBody] RejectLeaveDto dto)
        {
            try
            {
                var approvedById = GetCurrentUserId();
                var leave = await _leaveService.RejectLeaveRequestAsync(leaveId, approvedById, dto.RejectionReason);
                return Ok(ApiResponse.Success("Leave request rejected successfully", leave));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }
    }
}
