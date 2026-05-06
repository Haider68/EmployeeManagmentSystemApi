namespace EmployeeManagmentSystemApi.Helpers
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ApiResponse<T> SuccessResponse(string message, T? data = default)
            => new() { Success = true, Message = message, Data = data };
    }

    public static class ApiResponse
    {
        public static ApiResponse<object> Error(string message)
            => new() { Success = false, Message = message };

        public static ApiResponse<T> Success<T>(string message, T? data = default)
            => new() { Success = true, Message = message, Data = data };
    }
}
