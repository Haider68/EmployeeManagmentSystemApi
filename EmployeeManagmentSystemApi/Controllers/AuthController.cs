using EmployeeManagmentSystemApi.DTOs.Admin;
using EmployeeManagmentSystemApi.Helpers;
using EmployeeManagmentSystemApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagmentSystemApi.Controllers
{
    [Route("api/v1/admin")]
    public class AuthController : BaseApiController
    {
        private readonly IAdminService _adminService;

        public AuthController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // POST api/v1/admin/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterAdminDto dto)
        {
            try
            {
                var result = await _adminService.RegisterAsync(dto);
                SetAuthCookies(result.AccessToken, result.RefreshToken);
                return StatusCode(201, ApiResponse.Success("User registered successfully", new
                {
                    User = result.User,
                    AccessToken = result.AccessToken
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // POST api/v1/admin/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _adminService.LoginAsync(dto);
                SetAuthCookies(result.AccessToken, result.RefreshToken);
                return Ok(ApiResponse.Success("Login successful", new
                {
                    User = result.User,
                    AccessToken = result.AccessToken
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // POST api/v1/admin/refresh-token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto? body)
        {
            try
            {
                var token = Request.Cookies["refreshToken"] ?? body?.RefreshToken;
                if (string.IsNullOrWhiteSpace(token))
                    return BadRequest(ApiResponse.Error("Refresh token is required"));

                var result = await _adminService.RefreshTokenAsync(token);
                SetAuthCookies(result.AccessToken, result.RefreshToken);
                return Ok(ApiResponse.Success("Token refreshed successfully", new
                {
                    AccessToken = result.AccessToken
                }));
            }
            catch (Exception ex)
            {
                return Unauthorized(ApiResponse.Error(ex.Message));
            }
        }

        // POST api/v1/admin/logout  [Auth required]
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var clearOptions = new CookieOptions { SameSite = SameSiteMode.None, Secure = true };
            Response.Cookies.Delete("accessToken", clearOptions);
            Response.Cookies.Delete("refreshToken", clearOptions);
            return Ok(ApiResponse.Success<object>("Logged out successfully"));
        }

        // GET api/v1/admin/me  [Auth required]
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            try
            {
                var userId = GetCurrentUserId();
                var userType = GetCurrentUserType();
                var user = await _adminService.GetMeAsync(userId, userType);
                return Ok(ApiResponse.Success("User retrieved successfully", new { User = user }));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {
                return NotFound(ApiResponse.Error(ex.Message));
            }
        }

        // PUT api/v1/admin/update-profile  [Auth required]
        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateAdminProfileDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var updated = await _adminService.UpdateProfileAsync(userId, dto);
                return Ok(ApiResponse.Success("Profile updated successfully", new { User = updated }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        // PUT api/v1/admin/update-password  [Auth required]
        [Authorize]
        [HttpPut("update-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _adminService.ChangePasswordAsync(userId, dto);
                return Ok(ApiResponse.Success<object>("Password updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }

        /// <summary>
        /// Sets both the accessToken (15 min) and refreshToken (7 days) as HttpOnly cookies.
        /// SameSite=None allows them to be sent on cross-origin requests from the frontend.
        /// </summary>
        private void SetAuthCookies(string accessToken, string refreshToken)
        {
            var baseOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,          // required by SameSite=None
                SameSite = SameSiteMode.None  // must be None for cross-origin (localhost:3000 → localhost:7162)
            };

            Response.Cookies.Append("accessToken", accessToken, new CookieOptions
            {
                HttpOnly = baseOptions.HttpOnly,
                Secure = baseOptions.Secure,
                SameSite = baseOptions.SameSite,
                Expires = DateTimeOffset.UtcNow.AddMinutes(15)
            });

            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = baseOptions.HttpOnly,
                Secure = baseOptions.Secure,
                SameSite = baseOptions.SameSite,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
        }
    }
}
