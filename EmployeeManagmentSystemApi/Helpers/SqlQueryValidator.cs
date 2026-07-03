using System.Text.RegularExpressions;

namespace EmployeeManagmentSystemApi.Helpers
{
    public static class SqlQueryValidator
    {
        private static readonly string[] BlockedKeywords =
        [
            "INSERT", "UPDATE", "DELETE", "DROP", "TRUNCATE", "ALTER", "CREATE",
            "EXEC", "EXECUTE", "MERGE", "GRANT", "REVOKE", "DENY", "BACKUP",
            "RESTORE", "SHUTDOWN", "DBCC", "OPENROWSET", "OPENQUERY", "BULK",
            "INTO", "OUTFILE", "LOAD_FILE"
        ];

        private static readonly string[] ForbiddenColumns =
        [
            "PASSWORDHASH", "FINGERPRINT", "REFRESHTOKEN"
        ];

        public static (bool IsValid, string NormalizedSql, string? Error) ValidateAndNormalize(string sql, int maxRows)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return (false, sql, "SQL query is empty.");

            var normalized = sql.Trim().TrimEnd(';').Trim();

            if (normalized.Contains(';'))
                return (false, normalized, "Multiple SQL statements are not allowed.");

            var upper = normalized.ToUpperInvariant();

            if (!upper.StartsWith("SELECT"))
                return (false, normalized, "Only SELECT queries are allowed.");

            if (upper.Contains("SELECT INTO"))
                return (false, normalized, "SELECT INTO is not allowed.");

            foreach (var keyword in BlockedKeywords)
            {
                if (ContainsKeyword(upper, keyword))
                    return (false, normalized, $"Blocked SQL keyword: {keyword}");
            }

            if (upper.Contains("XP_"))
                return (false, normalized, "Extended stored procedures are not allowed.");

            foreach (var column in ForbiddenColumns)
            {
                if (upper.Contains(column))
                    return (false, normalized, $"Access to column '{column}' is forbidden.");
            }

            normalized = EnsureTopClause(normalized, maxRows);

            return (true, normalized, null);
        }

        private static bool ContainsKeyword(string upperSql, string keyword)
        {
            return Regex.IsMatch(upperSql, $@"\b{Regex.Escape(keyword)}\b", RegexOptions.IgnoreCase);
        }

        private static string EnsureTopClause(string sql, int maxRows)
        {
            if (Regex.IsMatch(sql, @"^\s*SELECT\s+TOP\s+\d+", RegexOptions.IgnoreCase))
                return sql;

            if (Regex.IsMatch(sql, @"^\s*SELECT\s+DISTINCT\s+", RegexOptions.IgnoreCase))
            {
                return Regex.Replace(
                    sql,
                    @"^\s*SELECT\s+DISTINCT\s+",
                    $"SELECT DISTINCT TOP {maxRows} ",
                    RegexOptions.IgnoreCase);
            }

            return Regex.Replace(sql, @"^\s*SELECT\s+", $"SELECT TOP {maxRows} ", RegexOptions.IgnoreCase);
        }
    }
}
