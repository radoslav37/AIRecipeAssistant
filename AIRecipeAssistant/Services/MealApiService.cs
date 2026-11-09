using System.Text.Json;
using AIRecipeAssistant.Models;

namespace AIRecipeAssistant.Services
{
    public class MealApiService
    {
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public MealApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<Meal>> SearchByIngredientsAsync(List<string> ingredients)
        {
            var apiKey = _configuration["Spoonacular:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Spoonacular API ключът не е настроен.");
            }

            var ingredientList = Uri.EscapeDataString(string.Join(",", ingredients));
            var url = $"https://api.spoonacular.com/recipes/findByIngredients?ingredients={ingredientList}&number=10&apiKey={Uri.EscapeDataString(apiKey)}";

            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<List<Meal>>(json, JsonOptions)
                ?? new List<Meal>();
        }
    }
}
