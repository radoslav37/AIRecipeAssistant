using AIRecipeAssistant.Models;
using AIRecipeAssistant.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIRecipeAssistant.Controllers
{
    public class RecipesController : Controller
    {
        private readonly MealApiService _mealApiService;
        private readonly JsonStorageService _jsonStorageService;
        private readonly GeminiService _geminiService;

        public RecipesController(
    MealApiService mealApiService,
    JsonStorageService jsonStorageService,
    GeminiService geminiService)
        {
            _mealApiService = mealApiService;
            _jsonStorageService = jsonStorageService;
            _geminiService = geminiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(string ingredients)
        {
            var ingredientList = (ingredients ?? "")
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

            var cacheKey = string.Join(", ",
                ingredientList
                    .Select(item => item.ToLowerInvariant())
                    .OrderBy(item => item));

            var cachedSearch = await _jsonStorageService.FindCacheAsync(cacheKey);

            List<Meal> meals;

            if (cachedSearch != null)
            {
                meals = cachedSearch.Recipes;
            }
            else
            {
                meals = await _mealApiService.SearchByIngredientsAsync(ingredientList);


                await _jsonStorageService.SaveAsync(new SearchHistory
                {
                    CacheKey = cacheKey,
                    Ingredients = string.Join(", ", ingredientList),
                    Recipes = meals,
                    CreatedAt = DateTime.Now
                });
            }

            ViewBag.Ingredients = string.Join(", ", ingredientList);
            return View("SearchResults", meals);
        }
    }
}