using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Garage.WebClient.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiUrl = "https://localhost:7200/api/Auth/login";

        public LoginController(IHttpClientFactory factory)
        {
            _client = factory.CreateClient();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string email, string password)
        {
            var loginData = new { Username = email, Password = password };

            var response = await _client.PostAsJsonAsync(_apiUrl, loginData);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Невалиден имейл или парола.";
                return View();
            }

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var token = json.RootElement.GetProperty("token").GetString();

           
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var username = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username ?? ""),
                new Claim(ClaimTypes.Role, role ?? "")
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

          
            await HttpContext.SignInAsync("CookieAuth", principal);

           
            HttpContext.Session.SetString("token", token!);

            
            return role switch
            {
                "Admin" => RedirectToAction("Index", "Dashboard"),
                "Client" => RedirectToAction("MyRepairs", "Repairs"),
                _ => RedirectToAction("AccessDenied", "Home")
            };
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}
