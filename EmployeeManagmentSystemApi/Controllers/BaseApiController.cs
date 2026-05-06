using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagmentSystemApi.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Returns the authenticated user's integer ID from the JWT "sub" claim.
        /// Handles both the raw "sub" name and the legacy mapped NameIdentifier form.
        /// </summary>
        protected int GetCurrentUserId()
        {
            // After clearing DefaultInboundClaimTypeMap, the claim stays as "sub"
            var value = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                     ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(value))
            {
                // Last-resort: scan all claims for any that look like an integer user id
                var allClaims = User.Claims.ToList();
                foreach (var c in allClaims)
                {
                    if (int.TryParse(c.Value, out _) &&
                        (c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase) ||
                         c.Type == "sub"))
                    {
                        value = c.Value;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(value) || !int.TryParse(value, out var userId))
                throw new UnauthorizedAccessException("User identity could not be determined from token.");

            return userId;
        }

        protected string GetCurrentUserType() =>
            User.FindFirstValue("userType") ?? "employee";

        protected string GetCurrentUserEmail() =>
            User.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? User.FindFirstValue(ClaimTypes.Email)
            ?? string.Empty;
    }
}
