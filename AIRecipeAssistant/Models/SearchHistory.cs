namespace AIRecipeAssistant.Models
{
    public class SearchHistory
    {
        public int Id { get; set; }
        public string Ingredients { get; set; } = "";
        public string RecipeNames { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}