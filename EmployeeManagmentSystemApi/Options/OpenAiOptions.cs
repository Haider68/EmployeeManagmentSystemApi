namespace EmployeeManagmentSystemApi.Options
{
    public class OpenAiOptions
    {
        public const string SectionName = "OpenAI";

        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-4.1-mini";
        public int MaxTokens { get; set; } = 4096;
        public int MaxQueryRows { get; set; } = 100;
        public int MaxToolRounds { get; set; } = 3;
        public int MaxHistoryMessages { get; set; } = 10;
        public int QueryTimeoutSeconds { get; set; } = 5;
    }
}
