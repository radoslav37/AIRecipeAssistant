using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AIRecipeAssistant.Services
{
    public class GroqService
    {
        public const string UnavailableMessage =
            "В момента ИИ услугата не работи. Опитай отново по-късно.";

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GroqService> _logger;

        public GroqService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GroqService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> SuggestRecipeIdeaAsync(string ingredients)
        {
            var apiKey = _configuration["Groq:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("Groq API key is not configured.");
                return UnavailableMessage;
            }

            var prompt =
                $"Предложи едно подходящо домашно ястие, като разгледаш тези налични продукти: {ingredients}. " +
                "Отговори изцяло на български и използвай точно тази структура:\n" +
                "Ястие: кратко и ясно име или описание с максимум 6 думи.\n" +
                "Допълнителни продукти: изброи само продуктите, които трябва да се купят, защото липсват. " +
                "Не включвай основни подправки като сол, пипер и олио. Ако не липсва нищо съществено, напиши „Няма“.\n" +
                "Време за приготвяне: посочи реалистично общо време в минути, включително измиване, белене, рязане и подготовка на продуктите, както и готвенето.\n" +
                "Приготвяне: опиши от 3 до 7 кратки, лесни и номерирани стъпки.\n" +
                "Не използвай всички налични продукти на всяка цена: пропусни тези, които не си подхождат или не са нужни за избраното ястие. " +
                "Ако наличните продукти не са достатъчни за добро ястие, добави в списъка липсващите съществени продукти. " +
                "Предпочитай прости рецепти и не добавяй излишни покупки.";

            var requestBody = new
            {
                model = "openai/gpt-oss-20b",
                messages = new object[]
                {
                    new { role = "system", content = "Ти си помощник за лесни домашни рецепти. Отговаряй кратко и на български." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 500,
                reasoning_effort = "low",
                temperature = 0.7
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.groq.com/openai/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            try
            {
                using var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Groq returned HTTP {StatusCode}.", (int)response.StatusCode);
                    return UnavailableMessage;
                }

                await using var responseStream = await response.Content.ReadAsStreamAsync();
                using var document = await JsonDocument.ParseAsync(responseStream);

                var answer = document.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString()
                    ?.Trim();

                return string.IsNullOrWhiteSpace(answer) ? UnavailableMessage : answer;
            }
            catch (OperationCanceledException exception)
            {
                _logger.LogWarning(exception, "Groq request timed out or was canceled.");
                return UnavailableMessage;
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "Groq request failed.");
                return UnavailableMessage;
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(exception, "Groq returned an invalid response.");
                return UnavailableMessage;
            }
            catch (KeyNotFoundException exception)
            {
                _logger.LogWarning(exception, "Groq response did not contain the expected answer.");
                return UnavailableMessage;
            }
            catch (InvalidOperationException exception)
            {
                _logger.LogWarning(exception, "Groq response did not contain a text answer.");
                return UnavailableMessage;
            }
        }
    }
}
