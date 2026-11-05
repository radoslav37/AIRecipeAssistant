using Microsoft.AspNetCore.Mvc;

namespace AIRecipeAssistant.Controllers
{
    public class RecipesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}