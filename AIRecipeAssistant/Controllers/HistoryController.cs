using Microsoft.AspNetCore.Mvc;

namespace AIRecipeAssistant.Controllers
{
    public class HistoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

