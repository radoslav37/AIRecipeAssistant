using System.Text.Json;
using AIRecipeAssistant.Models;

namespace AIRecipeAssistant.Services
{
    public class JsonStorageService
    {
        private readonly string _filePath =
            Path.Combine(Directory.GetCurrentDirectory(), "searchHistory.json");

        public async Task<List<SearchHistory>> GetAllAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new List<SearchHistory>();
            }

            var json = await File.ReadAllTextAsync(_filePath);

            return JsonSerializer.Deserialize<List<SearchHistory>>(json)
                   ?? new List<SearchHistory>();
        }

        public async Task SaveAsync(SearchHistory item)
        {
            var items = await GetAllAsync();

            item.Id = items.Count == 0 ? 1 : items.Max(x => x.Id) + 1;
            items.Add(item);

            var json = JsonSerializer.Serialize(items, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}