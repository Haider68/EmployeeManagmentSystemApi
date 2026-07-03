using System.ComponentModel.DataAnnotations;

namespace EmployeeManagmentSystemApi.DTOs.Chat
{
    public class ChatRequestDto
    {
        [Required]
        [MinLength(1)]
        public string Message { get; set; } = string.Empty;

        public List<ChatMessageDto>? History { get; set; }
    }
}
