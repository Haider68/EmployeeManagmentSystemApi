using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EmployeeManagmentSystemApi.DTOs.Chat;
using EmployeeManagmentSystemApi.Helpers;
using EmployeeManagmentSystemApi.Interfaces;
using EmployeeManagmentSystemApi.Options;
using Microsoft.Extensions.Options;

namespace EmployeeManagmentSystemApi.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDatabaseQueryService _queryService;
        private readonly OpenAiOptions _options;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public OpenAiService(
            IHttpClientFactory httpClientFactory,
            IDatabaseQueryService queryService,
            IOptions<OpenAiOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _queryService = queryService;
            _options = options.Value;
        }

        public async Task<ChatResponseDto> ProcessChatAsync(string message, List<ChatMessageDto>? history)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
                throw new InvalidOperationException("OpenAI API key is not configured. Set OpenAI:ApiKey in User Secrets or OpenAI__ApiKey environment variable.");

            var executedQueries = new List<string>();
            var totalRowCount = 0;

            var inputItems = BuildInputItems(message, history);
            string? previousResponseId = null;

            for (var round = 0; round < _options.MaxToolRounds; round++)
            {
                var requestBody = BuildRequestBody(inputItems, previousResponseId);
                using var response = await SendRequestAsync(requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    throw new InvalidOperationException(ParseApiError(response.StatusCode, errorBody));
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                previousResponseId = root.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;

                var functionCalls = ExtractFunctionCalls(root);
                if (functionCalls.Count == 0)
                {
                    var reply = ExtractTextOutput(root);
                    return new ChatResponseDto
                    {
                        Reply = string.IsNullOrWhiteSpace(reply)
                            ? "I could not generate a response. Please try rephrasing your question."
                            : reply,
                        ExecutedQueries = executedQueries,
                        RowCount = totalRowCount
                    };
                }

                inputItems = new List<object>();
                foreach (var call in functionCalls)
                {
                    var sql = ExtractSqlFromArguments(call.Arguments);
                    var queryResult = await _queryService.ExecuteReadOnlyQueryAsync(sql);
                    executedQueries.Add(queryResult.ExecutedSql);

                    if (queryResult.Success)
                        totalRowCount += queryResult.Rows.Count;

                    var outputPayload = queryResult.Success
                        ? JsonSerializer.Serialize(new
                        {
                            success = true,
                            row_count = queryResult.Rows.Count,
                            rows = queryResult.Rows
                        })
                        : JsonSerializer.Serialize(new
                        {
                            success = false,
                            error = queryResult.Error
                        });

                    inputItems.Add(new
                    {
                        type = "function_call_output",
                        call_id = call.CallId,
                        output = outputPayload
                    });
                }
            }

            return new ChatResponseDto
            {
                Reply = "I needed too many database queries to answer that. Please try a simpler question.",
                ExecutedQueries = executedQueries,
                RowCount = totalRowCount
            };
        }

        private List<object> BuildInputItems(string message, List<ChatMessageDto>? history)
        {
            var items = new List<object>();

            if (history != null)
            {
                foreach (var item in history.TakeLast(_options.MaxHistoryMessages))
                {
                    var role = item.Role.ToLowerInvariant() switch
                    {
                        "assistant" => "assistant",
                        "system" => "system",
                        _ => "user"
                    };

                    items.Add(new
                    {
                        role,
                        content = item.Content
                    });
                }
            }

            items.Add(new { role = "user", content = message });
            return items;
        }

        private object BuildRequestBody(List<object> input, string? previousResponseId)
        {
            var body = new Dictionary<string, object?>
            {
                ["model"] = _options.Model,
                ["instructions"] = DatabaseSchemaProvider.GetSchemaDescription(),
                ["input"] = input,
                ["max_output_tokens"] = _options.MaxTokens,
                ["tools"] = new[] { BuildQueryDatabaseTool() }
            };

            if (!string.IsNullOrEmpty(previousResponseId))
                body["previous_response_id"] = previousResponseId;

            return body;
        }

        /// <summary>
        /// Dictionary keys must be used (not anonymous types) so JsonSerializer snake_case
        /// policy does not rename JSON Schema fields like "additionalProperties".
        /// </summary>
        private static Dictionary<string, object> BuildQueryDatabaseTool()
        {
            return new Dictionary<string, object>
            {
                ["type"] = "function",
                ["name"] = "query_database",
                ["description"] = "Run a read-only T-SQL SELECT query against the HR database. Use for any data question.",
                ["strict"] = true,
                ["parameters"] = new Dictionary<string, object>
                {
                    ["type"] = "object",
                    ["properties"] = new Dictionary<string, object>
                    {
                        ["sql"] = new Dictionary<string, object>
                        {
                            ["type"] = "string",
                            ["description"] = "A single T-SQL SELECT statement. Must not modify data."
                        }
                    },
                    ["required"] = new[] { "sql" },
                    ["additionalProperties"] = false
                }
            };
        }

        private async Task<HttpResponseMessage> SendRequestAsync(object requestBody)
        {
            var json = JsonSerializer.Serialize(requestBody, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient(OpenAiHttpClientExtensions.ClientName);
            using var request = new HttpRequestMessage(HttpMethod.Post, "v1/responses");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            request.Content = content;

            return await client.SendAsync(request);
        }

        private static List<FunctionCallInfo> ExtractFunctionCalls(JsonElement root)
        {
            var calls = new List<FunctionCallInfo>();

            if (!root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
                return calls;

            foreach (var item in output.EnumerateArray())
            {
                if (!item.TryGetProperty("type", out var typeProp))
                    continue;

                var type = typeProp.GetString();
                if (type != "function_call")
                    continue;

                var callId = item.TryGetProperty("call_id", out var callIdProp) ? callIdProp.GetString() ?? "" : "";
                var name = item.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "" : "";
                var arguments = item.TryGetProperty("arguments", out var argsProp) ? argsProp.GetString() ?? "{}" : "{}";

                if (name == "query_database")
                    calls.Add(new FunctionCallInfo(callId, arguments));
            }

            return calls;
        }

        private static string ExtractTextOutput(JsonElement root)
        {
            if (!root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
                return string.Empty;

            var parts = new List<string>();

            foreach (var item in output.EnumerateArray())
            {
                if (!item.TryGetProperty("type", out var typeProp))
                    continue;

                if (typeProp.GetString() != "message")
                    continue;

                if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var block in content.EnumerateArray())
                {
                    if (block.TryGetProperty("type", out var blockType) &&
                        blockType.GetString() == "output_text" &&
                        block.TryGetProperty("text", out var textProp))
                    {
                        parts.Add(textProp.GetString() ?? "");
                    }
                }
            }

            if (parts.Count > 0)
                return string.Join("\n", parts);

            if (root.TryGetProperty("output_text", out var outputText))
                return outputText.GetString() ?? string.Empty;

            return string.Empty;
        }

        private static string ExtractSqlFromArguments(string argumentsJson)
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            if (doc.RootElement.TryGetProperty("sql", out var sqlProp))
                return sqlProp.GetString() ?? throw new InvalidOperationException("Missing sql in tool arguments.");

            throw new InvalidOperationException("Missing sql in tool arguments.");
        }

        private static string ParseApiError(System.Net.HttpStatusCode statusCode, string body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("error", out var error) &&
                    error.TryGetProperty("message", out var message))
                {
                    return $"OpenAI API error ({(int)statusCode}): {message.GetString()}";
                }
            }
            catch
            {
                // fall through
            }

            return $"OpenAI API error ({(int)statusCode}): {body}";
        }

        private sealed record FunctionCallInfo(string CallId, string Arguments);
    }
}
