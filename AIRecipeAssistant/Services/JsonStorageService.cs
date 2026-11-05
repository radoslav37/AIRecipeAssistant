using System.Text.Json;
using AIRecipeAssistant.Models;

namespace AIRecipeAssistant.Services
{
    public class JsonStorageService
    {
        private readonly string _filePath =
            Path.Combine(Directory.GetCurrentDirectory(), "searchHistory.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public async Task<List<SearchHistory>> GetAllAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new List<SearchHistory>();
            }

            var json = await File.ReadAllTextAsync(_filePath);

            return JsonSerializer.Deserialize<List<SearchHistory>>(json, JsonOptions)
                   ?? new List<SearchHistory>();
        }

        public async Task<SearchHistory?> FindCacheAsync(string cacheKey)
        {
            var items = await GetAllAsync();

            return items
                .Where(item => item.Recipes.Count > 0)
                .LastOrDefault(item =>
                    string.Equals(item.CacheKey, cacheKey, StringComparison.OrdinalIgnoreCase));
        }

        public async Task SaveAsync(SearchHistory item)
        {
            var items = await GetAllAsync();

            var existing = items.LastOrDefault(x =>
                string.Equals(x.CacheKey, item.CacheKey, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.Ingredients = item.Ingredients;
                existing.Recipes = item.Recipes;
                existing.CreatedAt = item.CreatedAt;
            }
            else
            {
                item.Id = items.Count == 0 ? 1 : items.Max(x => x.Id) + 1;
                items.Add(item);
            }

            var json = JsonSerializer.Serialize(items, JsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}