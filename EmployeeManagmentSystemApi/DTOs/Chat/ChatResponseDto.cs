namespace EmployeeManagmentSystemApi.DTOs.Chat
{
    public class ChatResponseDto
    {
        public string Reply { get; set; } = string.Empty;
        public List<string> ExecutedQueries { get; set; } = new();
        public int RowCount { get; set; }
    }
}
