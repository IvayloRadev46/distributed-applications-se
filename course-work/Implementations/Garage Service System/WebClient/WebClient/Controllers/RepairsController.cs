using Garage.WebClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Garage.WebClient.Controllers
{
    public class RepairsController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _repairsApiUrl = "https://localhost:7200/api/Repairs";
        private readonly string _carsApiUrl = "https://localhost:7200/api/Cars";

        public RepairsController(IHttpClientFactory factory)
        {
            _client = factory.CreateClient();
        }

        public async Task<IActionResult> Index(string? query, string? sort = "date", string? order = "asc", int page = 1, int pageSize = 10)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Index", "Login");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var repairsResponse = await _client.GetAsync(_repairsApiUrl);
            var carsResponse = await _client.GetAsync(_carsApiUrl);

            if (!repairsResponse.IsSuccessStatusCode || !carsResponse.IsSuccessStatusCode)
            {
                ViewBag.Error = "Грешка при зареждане на ремонтите.";
                return View(new List<RepairModel>());
            }

            var repairs = await repairsResponse.Content.ReadFromJsonAsync<List<RepairModel>>() ?? new();
            var cars = await carsResponse.Content.ReadFromJsonAsync<List<CarModel>>() ?? new();

            foreach (var repair in repairs)
            {
                var car = cars.FirstOrDefault(c => c.Id == repair.CarId);
                repair.CarModel = car != null ? $"{car.Model} ({car.LicensePlate})" : "Непозната кола";
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                var words = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                repairs = repairs.Where(r =>
                    words.All(word =>
                        (r.Description?.ToLower().Contains(word) ?? false) ||
                        (r.Status?.ToLower().Contains(word) ?? false) ||
                        (r.CarModel?.ToLower().Contains(word) ?? false) ||
                        r.StartDate.ToString("yyyy-MM-dd").Contains(word)
                    )
                ).ToList();
            }

            bool ascending = order != "desc";
            repairs = (sort, ascending) switch
            {
                ("description", true) => repairs.OrderBy(r => r.Description).ToList(),
                ("description", false) => repairs.OrderByDescending(r => r.Description).ToList(),
                ("date", true) => repairs.OrderBy(r => r.StartDate).ToList(),
                ("date", false) => repairs.OrderByDescending(r => r.StartDate).ToList(),
                ("status", true) => repairs.OrderBy(r => r.Status).ToList(),
                ("status", false) => repairs.OrderByDescending(r => r.Status).ToList(),
                ("car", true) => repairs.OrderBy(r => r.CarModel).ToList(),
                ("car", false) => repairs.OrderByDescending(r => r.CarModel).ToList(),
                _ => repairs
            };

            int totalItems = repairs.Count;
            var pagedRepairs = repairs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentSort = sort;
            ViewBag.CurrentOrder = order;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.Query = query;
            ViewBag.PageSize = pageSize;

            return View(pagedRepairs);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
        
            var cars = await GetCarsAsync();
            ViewBag.Cars = new SelectList(cars, "Id", "ClientFullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(RepairModel repair)
        {
       

            if (!ModelState.IsValid)
            {
                var cars = await GetCarsAsync();
                ViewBag.Cars = new SelectList(cars, "Id", "ClientFullName", repair.CarId);
                return View(repair);
            }

            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Index", "Login");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.PostAsJsonAsync(_repairsApiUrl, repair);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var statusCode = (int)response.StatusCode;
            var errorMessage = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"Грешка при създаване на ремонт (Статус: {statusCode}): {errorMessage}";
            var retryCars = await GetCarsAsync();
            ViewBag.Cars = new SelectList(retryCars, "Id", "ClientFullName", repair.CarId);
            return View(repair);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
         

            var token = HttpContext.Session.GetString("token");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync($"{_repairsApiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return RedirectToAction(nameof(Index));

            var repair = await response.Content.ReadFromJsonAsync<RepairModel>();
            var cars = await GetCarsAsync();
            ViewBag.Cars = new SelectList(cars, "Id", "ClientFullName", repair?.CarId);
            return View(repair);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RepairModel repair)
        {
         

            if (!ModelState.IsValid)
            {
                var cars = await GetCarsAsync();
                ViewBag.Cars = new SelectList(cars, "Id", "ClientFullName", repair.CarId);
                return View(repair);
            }

            var token = HttpContext.Session.GetString("token");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PutAsJsonAsync($"{_repairsApiUrl}/{repair.Id}", repair);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var statusCode = (int)response.StatusCode;
            var errorMessage = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"Грешка при редактиране на ремонт (Статус: {statusCode}): {errorMessage}";
            var retryCars = await GetCarsAsync();
            ViewBag.Cars = new SelectList(retryCars, "Id", "ClientFullName", repair.CarId);
            return View(repair);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
        

            var token = HttpContext.Session.GetString("token");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            await _client.DeleteAsync($"{_repairsApiUrl}/{id}");
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<CarModel>> GetCarsAsync()
        {
            var token = HttpContext.Session.GetString("token");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.GetAsync(_carsApiUrl);

            var cars = await response.Content.ReadFromJsonAsync<List<CarModel>>() ?? new();
            foreach (var car in cars)
            {
                car.ClientFullName = $"{car.Model} ({car.LicensePlate})";
            }
            return cars;
        }
        [Authorize(Roles = "Client")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyRepairs()
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Index", "Login");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var userEmail = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            
            var carsResponse = await _client.GetAsync("https://localhost:7200/api/Cars");
            var repairsResponse = await _client.GetAsync("https://localhost:7200/api/Repairs");
            var clientsResponse = await _client.GetAsync("https://localhost:7200/api/Clients");

            if (!carsResponse.IsSuccessStatusCode || !repairsResponse.IsSuccessStatusCode || !clientsResponse.IsSuccessStatusCode)
            {
                ViewBag.Error = "Грешка при зареждане на данни.";
                return View(new List<RepairModel>());
            }

            var cars = await carsResponse.Content.ReadFromJsonAsync<List<CarModel>>() ?? new();
            var repairs = await repairsResponse.Content.ReadFromJsonAsync<List<RepairModel>>() ?? new();
            var clients = await clientsResponse.Content.ReadFromJsonAsync<List<ClientModel>>() ?? new();

           
            var client = clients.FirstOrDefault(c => c.Email == userEmail);
            if (client == null) return View(new List<RepairModel>());

            
            var clientCarIds = cars.Where(c => c.ClientId == client.Id).Select(c => c.Id).ToList();
            var myRepairs = repairs
                .Where(r => clientCarIds.Contains(r.CarId))
                .ToList();

           
            foreach (var repair in myRepairs)
            {
                var car = cars.FirstOrDefault(c => c.Id == repair.CarId);
                repair.CarModel = car != null ? $"{car.Model} ({car.LicensePlate})" : "Непозната кола";
            }

            return View(myRepairs);
        }


    }
}
