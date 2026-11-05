using AIRecipeAssistant.Models;
using AIRecipeAssistant.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIRecipeAssistant.Controllers
{
    public class RecipesController : Controller
    {
        private readonly MealApiService _mealApiService;
        private readonly JsonStorageService _jsonStorageService;

        public RecipesController(
            MealApiService mealApiService,
            JsonStorageService jsonStorageService)
        {
            _mealApiService = mealApiService;
            _jsonStorageService = jsonStorageService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(string ingredients)
        {
            var ingredientList = ingredients
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (ingredientList.Count == 0)
            {
                ModelState.AddModelError("", "Въведи поне един продукт.");
                return View("Index");
            }

            var meals = await _mealApiService.SearchByIngredientsAsync(ingredientList);

            await _jsonStorageService.SaveAsync(new SearchHistory
            {
                Ingredients = string.Join(", ", ingredientList),
                RecipeNames = string.Join(", ", meals.Select(meal => meal.Title)),
                CreatedAt = DateTime.Now
            });

            ViewBag.Ingredients = string.Join(", ", ingredientList);
            return View("SearchResults", meals);
        }
    }
}