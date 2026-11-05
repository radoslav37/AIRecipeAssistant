namespace AIRecipeAssistant.Models
{
    public class SearchHistory
    {
        public int Id { get; set; }

        // Нормализиран ключ за проверка на кеша, например "cheese, eggs".
        public string CacheKey { get; set; } = "";

        // Продуктите, въведени от потребителя.
        public string Ingredients { get; set; } = "";

        // Пълните резултати, включително URL адресите на снимките.
        public List<Meal> Recipes { get; set; } = new();
        public string AiRecipeIdea { get; set; } = "";

        public DateTime CreatedAt { get; set; }
        
    }
}