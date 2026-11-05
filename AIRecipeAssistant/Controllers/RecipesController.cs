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

            var ingredientsText = string.Join(", ", ingredientList);

            var cacheKey = string.Join(", ",
                ingredientList
                    .Select(item => item.ToLowerInvariant())
                    .OrderBy(item => item));

            var cachedSearch = await _jsonStorageService.FindCacheAsync(cacheKey);

            SearchHistory search;

            if (cachedSearch != null)
            {
                search = cachedSearch;

                // Добавя AI идея и към кеширан запис, който още няма такава.
                if (string.IsNullOrWhiteSpace(search.AiRecipeIdea))
                {
                    search.AiRecipeIdea =
                        await _geminiService.SuggestRecipeIdeaAsync(ingredientsText);

                    await _jsonStorageService.SaveAsync(search);
                }
            }
            else
            {
                var meals = await _mealApiService.SearchByIngredientsAsync(ingredientList);
                var aiIdea = await _geminiService.SuggestRecipeIdeaAsync(ingredientsText);

                search = new SearchHistory
                {
                    CacheKey = cacheKey,
                    Ingredients = ingredientsText,
                    Recipes = meals,
                    AiRecipeIdea = aiIdea,
                    CreatedAt = DateTime.Now
                };

                await _jsonStorageService.SaveAsync(search);
            }

            ViewBag.Ingredients = ingredientsText;
            ViewBag.AiRecipeIdea = search.AiRecipeIdea;

            return View("SearchResults", search.Recipes);
        }
    }
}