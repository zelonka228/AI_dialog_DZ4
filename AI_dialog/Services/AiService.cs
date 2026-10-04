using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AI_dialog.Services;

public class AiService
{
    private readonly HttpClient _httpClient = new();

    private const string ApiKey = ApiKeys.OpenRouter;

    public async Task<string> AskAi(string prompt)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", ApiKey);

        var request = new
        {
            model = "openrouter/free",
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = prompt
                }
            }
        };

        var json = JsonSerializer.Serialize(request);

        using var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync(
            "https://openrouter.ai/api/v1/chat/completions",
            content
        );

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(responseJson);

        return document
            .RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "";
    }
}
