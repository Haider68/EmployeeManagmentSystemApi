namespace EmployeeManagmentSystemApi.Helpers
{
    public static class OpenAiHttpClientExtensions
    {
        public const string ClientName = "OpenAI";

        public static IServiceCollection AddOpenAiHttpClient(this IServiceCollection services)
        {
            services.AddHttpClient(ClientName, ConfigureClient);
            return services;
        }

        private static void ConfigureClient(HttpClient client)
        {
            client.BaseAddress = new Uri("https://api.openai.com/");
            client.Timeout = TimeSpan.FromSeconds(120);
        }
    }
}
