namespace AIRecipeAssistant.Models
{
    public class Meal
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Image { get; set; } = "";
        public List<Ingredient> MissedIngredients { get; set; } = new();
        public List<Ingredient> UsedIngredients { get; set; } = new();
    }

    public class Ingredient
    {
        public string Name { get; set; } = "";
    }
}