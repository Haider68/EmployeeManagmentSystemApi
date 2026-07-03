namespace EmployeeManagmentSystemApi.Interfaces
{
    public interface IDatabaseQueryService
    {
        Task<DatabaseQueryResult> ExecuteReadOnlyQueryAsync(string sql);
    }

    public class DatabaseQueryResult
    {
        public bool Success { get; set; }
        public List<Dictionary<string, object?>> Rows { get; set; } = new();
        public string? Error { get; set; }
        public string ExecutedSql { get; set; } = string.Empty;
    }
}
