using EmployeeManagmentSystemApi.DTOs.Chat;

namespace EmployeeManagmentSystemApi.Interfaces
{
    public interface IOpenAiService
    {
        Task<ChatResponseDto> ProcessChatAsync(string message, List<ChatMessageDto>? history);
    }
}
