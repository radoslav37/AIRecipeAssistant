using System.Text;
using System.Text.Json;

namespace AIRecipeAssistant.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> SuggestSubstitutionsAsync(
            string recipeTitle,
            List<string> missingIngredients)
        {
            if (missingIngredients.Count == 0)
            {
                return "Не липсват съставки.";
            }

            var apiKey = _configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return "Gemini API ключът не е настроен.";
            }

            var prompt =
                $"За рецептата '{recipeTitle}' липсват: {string.Join(", ", missingIngredients)}. " +
                "Предложи кратки и практични заместители на липсващите съставки на български. " +
                "Не добавяй увод.";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.8-flash:generateContent?key={Uri.EscapeDataString(apiKey)}";

            using var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();

                return $"Gemini грешка ({(int)response.StatusCode}): {errorBody}";
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);

            return document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString()
                ?.Trim()
                ?? "В момента няма предложение за замяна.";
        }
    }
}