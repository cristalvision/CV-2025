using Microsoft.AspNetCore.Mvc;

namespace CV_2025.Home
{
    public class Home : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
