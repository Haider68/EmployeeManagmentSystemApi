using EmployeeManagmentSystemApi.DTOs.Chat;
using EmployeeManagmentSystemApi.Interfaces;

namespace EmployeeManagmentSystemApi.Services
{
    public class ChatService : IChatService
    {
        private readonly IOpenAiService _openAiService;

        public ChatService(IOpenAiService openAiService)
        {
            _openAiService = openAiService;
        }

        public Task<ChatResponseDto> SendMessageAsync(ChatRequestDto request)
        {
            return _openAiService.ProcessChatAsync(request.Message, request.History);
        }
    }
}
