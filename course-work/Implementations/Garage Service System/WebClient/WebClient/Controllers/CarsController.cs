using Garage.WebClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Garage.WebClient.Controllers
{
    public class CarsController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _carsApiUrl = "https://localhost:7200/api/Cars";
        private readonly string _clientsApiUrl = "https://localhost:7200/api/Clients";
       
        public CarsController(IHttpClientFactory factory)
        {
            _client = factory.CreateClient();
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? query, string? sort = "model", string? order = "asc", int page = 1, int pageSize = 10)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Index", "Login");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var carsResponse = await _client.GetAsync(_carsApiUrl);
            var clientsResponse = await _client.GetAsync(_clientsApiUrl);

            if (!carsResponse.IsSuccessStatusCode || !clientsResponse.IsSuccessStatusCode)
            {
                ViewBag.Error = "Грешка при зареждане на данни.";
                return View(new List<CarModel>());
            }

            var cars = await carsResponse.Content.ReadFromJsonAsync<List<CarModel>>() ?? new();
            var clients = await clientsResponse.Content.ReadFromJsonAsync<List<ClientModel>>() ?? new();

            foreach (var car in cars)
            {
                var client = clients.FirstOrDefault(c => c.Id == car.ClientId);
                car.ClientFullName = client != null ? $"{client.FirstName} {client.LastName}" : "Неизвестен клиент";
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                var words = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                cars = cars.Where(car =>
                    words.All(word =>
                        (car.VIN?.ToLower().Contains(word) ?? false) ||
                        (car.LicensePlate?.ToLower().Contains(word) ?? false) ||
                        (car.Model?.ToLower().Contains(word) ?? false) ||
                        car.ManufactureYear.ToString().Contains(word) ||
                        (car.ClientFullName?.ToLower().Contains(word) ?? false)
                    )
                ).ToList();
            }

            bool ascending = order != "desc";
            cars = (sort, ascending) switch
            {
                ("vin", true) => cars.OrderBy(c => c.VIN).ToList(),
                ("vin", false) => cars.OrderByDescending(c => c.VIN).ToList(),
                ("licensePlate", true) => cars.OrderBy(c => c.LicensePlate).ToList(),
                ("licensePlate", false) => cars.OrderByDescending(c => c.LicensePlate).ToList(),
                ("model", true) => cars.OrderBy(c => c.Model).ToList(),
                ("model", false) => cars.OrderByDescending(c => c.Model).ToList(),
                ("year", true) => cars.OrderBy(c => c.ManufactureYear).ToList(),
                ("year", false) => cars.OrderByDescending(c => c.ManufactureYear).ToList(),
                ("client", true) => cars.OrderBy(c => c.ClientFullName).ToList(),
                ("client", false) => cars.OrderByDescending(c => c.ClientFullName).ToList(),
                _ => cars
            };

            int totalItems = cars.Count;
            var pagedCars = cars
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentSort = sort;
            ViewBag.CurrentOrder = order;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.Query = query;
            ViewBag.PageSize = pageSize;

            return View(pagedCars);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var clients = await GetClientsAsync();
            ViewBag.Clients = new SelectList(clients, "Id", "FullName");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarModel car)
        {
           

            if (!ModelState.IsValid)
            {
                var clients = await GetClientsAsync();
                ViewBag.Clients = new SelectList(clients, "Id", "FullName", car.ClientId);
                return View(car);
            }

            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Index", "Login");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.PostAsJsonAsync(_carsApiUrl, car);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var statusCode = (int)response.StatusCode;
            var errorDetails = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"Грешка при създаване на автомобил (Статус: {statusCode}): {errorDetails}";

            var retryClients = await GetClientsAsync();
            ViewBag.Clients = new SelectList(retryClients, "Id", "FullName", car.ClientId);
            return View(car);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
          

            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Index", "Login");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.GetAsync($"{_carsApiUrl}/{id}");

            if (!response.IsSuccessStatusCode) return RedirectToAction("Index");

            var car = await response.Content.ReadFromJsonAsync<CarModel>();
            var clients = await GetClientsAsync();
            ViewBag.Clients = new SelectList(clients, "Id", "FullName", car?.ClientId);
            return View(car);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(CarModel car)
        {
           

            if (!ModelState.IsValid)
            {
                var clients = await GetClientsAsync();
                ViewBag.Clients = new SelectList(clients, "Id", "FullName", car.ClientId);
                return View(car);
            }

            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Index", "Login");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.PutAsJsonAsync($"{_carsApiUrl}/{car.Id}", car);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var statusCode = (int)response.StatusCode;
            var errorDetails = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"Грешка при редакция на автомобил (Статус: {statusCode}): {errorDetails}";

            var retryClients = await GetClientsAsync();
            ViewBag.Clients = new SelectList(retryClients, "Id", "FullName", car.ClientId);
            return View(car);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            

            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Index", "Login");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            await _client.DeleteAsync($"{_carsApiUrl}/{id}");

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Admin")]
        private async Task<List<ClientModel>> GetClientsAsync()
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token)) return new List<ClientModel>();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.GetAsync(_clientsApiUrl);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<ClientModel>>() ?? new()
                : new List<ClientModel>();
        }
    }
}
