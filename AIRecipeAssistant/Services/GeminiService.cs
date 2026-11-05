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

        public async Task<string> SuggestRecipeIdeaAsync(string ingredients)
        {
            const string unavailableMessage =
                "В момента ИИ услугата не работи. Опитай отново по-късно.";

            var apiKey = _configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return unavailableMessage;
            }

            var prompt =
                $"Предложи едно лесно ястие, което може да се приготви с тези продукти: {ingredients}. " +
                "Отговори на български. Дай име на ястието и кратки инструкции в 3-5 стъпки. " +
                "Използвай основно изброените продукти и посочи, ако са нужни основни подправки.";

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

            try
            {
                using var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    return unavailableMessage;
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
                    ?? unavailableMessage;
            }
            catch (TaskCanceledException)
            {
                return unavailableMessage;
            }
            catch (HttpRequestException)
            {
                return unavailableMessage;
            }
            catch (JsonException)
            {
                return unavailableMessage;
            }
            catch (KeyNotFoundException)
            {
                return unavailableMessage;
            }
            catch (InvalidOperationException)
            {
                return unavailableMessage;
            }
        }
    }
}