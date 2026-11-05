using AIRecipeAssistant.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIRecipeAssistant.Controllers
{
    public class HistoryController : Controller
    {
        private readonly JsonStorageService _jsonStorageService;

        public HistoryController(JsonStorageService jsonStorageService)
        {
            _jsonStorageService = jsonStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var history = await _jsonStorageService.GetAllAsync();
            return View(history);
        }
    }
}