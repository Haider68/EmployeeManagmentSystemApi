using EmployeeManagmentSystemApi.DTOs.Chat;
using EmployeeManagmentSystemApi.Helpers;
using EmployeeManagmentSystemApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagmentSystemApi.Controllers
{
    [Route("api/v1/chat")]
    //[Authorize]
    public class ChatController : BaseApiController
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        // POST api/v1/chat/message
        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto dto)
        {
            try
            {
                if (GetCurrentUserType() != "admin")
                    return StatusCode(403, ApiResponse.Error("Only admins can use the AI chatbot."));

                var response = await _chatService.SendMessageAsync(dto);
                return Ok(ApiResponse.Success("Chat response generated", response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message));
            }
        }
    }
}
