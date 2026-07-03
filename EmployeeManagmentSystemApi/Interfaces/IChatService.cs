using EmployeeManagmentSystemApi.DTOs.Chat;

namespace EmployeeManagmentSystemApi.Interfaces
{
    public interface IChatService
    {
        Task<ChatResponseDto> SendMessageAsync(ChatRequestDto request);
    }
}
