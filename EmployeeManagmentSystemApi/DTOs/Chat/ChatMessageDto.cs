using System.ComponentModel.DataAnnotations;

namespace EmployeeManagmentSystemApi.DTOs.Chat
{
    public class ChatMessageDto
    {
        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;
    }
}
