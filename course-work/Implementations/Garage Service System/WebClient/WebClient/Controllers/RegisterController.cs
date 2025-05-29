using System.Net.Http.Json;
using Garage.WebClient.Models;
using Microsoft.AspNetCore.Mvc;

namespace Garage.WebClient.Controllers
{
    public class RegisterController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _authApiUrl = "https://localhost:7200/api/Auth/register";

        public RegisterController(IHttpClientFactory factory)
        {
            _client = factory.CreateClient();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ClientModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.RegistrationDate = DateTime.UtcNow;

            var response = await _client.PostAsJsonAsync(_authApiUrl, model);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ViewBag.Error = "Неуспешна регистрация: " + error;
                return View(model);
            }

            return RedirectToAction("Index", "Login");
        }
    }
}
