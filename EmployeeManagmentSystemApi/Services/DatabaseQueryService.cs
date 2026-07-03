using System.Data;
using EmployeeManagmentSystemApi.Data;
using EmployeeManagmentSystemApi.Helpers;
using EmployeeManagmentSystemApi.Interfaces;
using EmployeeManagmentSystemApi.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EmployeeManagmentSystemApi.Services
{
    public class DatabaseQueryService : IDatabaseQueryService
    {
        private readonly AppDbContext _db;
        private readonly OpenAiOptions _options;

        public DatabaseQueryService(AppDbContext db, IOptions<OpenAiOptions> options)
        {
            _db = db;
            _options = options.Value;
        }

        public async Task<DatabaseQueryResult> ExecuteReadOnlyQueryAsync(string sql)
        {
            var (isValid, normalizedSql, error) = SqlQueryValidator.ValidateAndNormalize(sql, _options.MaxQueryRows);
            if (!isValid)
            {
                return new DatabaseQueryResult
                {
                    Success = false,
                    Error = error,
                    ExecutedSql = sql
                };
            }

            var connection = _db.Database.GetDbConnection();
            var wasOpen = connection.State == ConnectionState.Open;

            if (!wasOpen)
                await connection.OpenAsync();

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = normalizedSql;
                command.CommandTimeout = _options.QueryTimeoutSeconds;

                await using var reader = await command.ExecuteReaderAsync();
                var rows = new List<Dictionary<string, object?>>();

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        row[reader.GetName(i)] = value;
                    }
                    rows.Add(row);
                }

                return new DatabaseQueryResult
                {
                    Success = true,
                    Rows = rows,
                    ExecutedSql = normalizedSql
                };
            }
            catch (Exception ex)
            {
                return new DatabaseQueryResult
                {
                    Success = false,
                    Error = ex.Message,
                    ExecutedSql = normalizedSql
                };
            }
            finally
            {
                if (!wasOpen && connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }
        }
    }
}
