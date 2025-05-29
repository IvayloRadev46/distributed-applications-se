using Microsoft.AspNetCore.Mvc;

namespace TermProject.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
