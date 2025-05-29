using Microsoft.AspNetCore.Mvc;
using Garage.WebClient.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;

public class ClientsController : Controller
{
    private readonly HttpClient _client;
    private readonly string _apiUrl = "https://localhost:7200/api/Clients";

    public ClientsController(IHttpClientFactory factory)
    {
        _client = factory.CreateClient();
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        

        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(ClientModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        model.RegistrationDate = DateTime.UtcNow;

        var token = HttpContext.Session.GetString("token");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Index", "Login");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync(_apiUrl, model);

        if (response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        ViewBag.Error = "Неуспешно създаване на клиент.";
        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
       

        var token = HttpContext.Session.GetString("token");
        if (string.IsNullOrEmpty(token)) return RedirectToAction("Index", "Login");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync($"{_apiUrl}/{id}");

        if (!response.IsSuccessStatusCode) return RedirectToAction("Index");

        var model = await response.Content.ReadFromJsonAsync<ClientModel>();
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(ClientModel model)
    {
      

        if (!ModelState.IsValid) return View(model);

        var token = HttpContext.Session.GetString("token");
        if (string.IsNullOrEmpty(token)) return RedirectToAction("Index", "Login");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PutAsJsonAsync($"{_apiUrl}/{model.Id}", model);

        if (response.IsSuccessStatusCode) return RedirectToAction("Index");

        ViewBag.Error = "Неуспешна редакция на клиента.";
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
       

        var token = HttpContext.Session.GetString("token");
        if (string.IsNullOrEmpty(token)) return RedirectToAction("Index", "Login");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.DeleteAsync($"{_apiUrl}/{id}");
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Index(string? query, string? sort = "firstName", string? order = "asc", int page = 1, int pageSize = 10)
    {
        var token = HttpContext.Session.GetString("token");
        if (string.IsNullOrEmpty(token)) return RedirectToAction("Index", "Login");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var clientsResponse = await _client.GetAsync(_apiUrl);
        var repairsResponse = await _client.GetAsync("https://localhost:7200/api/Repairs");
        var carsResponse = await _client.GetAsync("https://localhost:7200/api/Cars");

        if (!clientsResponse.IsSuccessStatusCode || !repairsResponse.IsSuccessStatusCode || !carsResponse.IsSuccessStatusCode)
        {
            ViewBag.Error = "Грешка при зареждане на клиентите или ремонтите.";
            return View(new List<ClientModel>());
        }

        var clients = await clientsResponse.Content.ReadFromJsonAsync<List<ClientModel>>() ?? new();
        var repairs = await repairsResponse.Content.ReadFromJsonAsync<List<RepairModel>>() ?? new();
        var cars = await carsResponse.Content.ReadFromJsonAsync<List<CarModel>>() ?? new();

     
        foreach (var client in clients)
        {
            var clientCarIds = cars.Where(c => c.ClientId == client.Id).Select(c => c.Id).ToList();
            var total = repairs.Where(r => clientCarIds.Contains(r.CarId)).Sum(r => r.Price);
            client.TotalSpent = total;
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var words = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            clients = clients.Where(c =>
                words.All(word =>
                    (c.FirstName?.ToLower().Contains(word) ?? false) ||
                    (c.LastName?.ToLower().Contains(word) ?? false) ||
                    (c.Email?.ToLower().Contains(word) ?? false) ||
                    (c.Phone?.ToLower().Contains(word) ?? false) ||
                    c.RegistrationDate.ToString("dd.MM.yyyy").Contains(word)
                )
            ).ToList();
        }

        bool ascending = order != "desc";
        clients = (sort, ascending) switch
        {
            ("firstName", true) => clients.OrderBy(c => c.FirstName).ToList(),
            ("firstName", false) => clients.OrderByDescending(c => c.FirstName).ToList(),
            ("lastName", true) => clients.OrderBy(c => c.LastName).ToList(),
            ("lastName", false) => clients.OrderByDescending(c => c.LastName).ToList(),
            ("email", true) => clients.OrderBy(c => c.Email).ToList(),
            ("email", false) => clients.OrderByDescending(c => c.Email).ToList(),
            ("registrationDate", true) => clients.OrderBy(c => c.RegistrationDate).ToList(),
            ("registrationDate", false) => clients.OrderByDescending(c => c.RegistrationDate).ToList(),
            ("loyaltyPoints", true) => clients.OrderBy(c => c.LoyaltyPoints).ToList(),
            ("loyaltyPoints", false) => clients.OrderByDescending(c => c.LoyaltyPoints).ToList(),
            _ => clients
        };

        int totalItems = clients.Count;
        var pagedClients = clients
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.CurrentSort = sort;
        ViewBag.CurrentOrder = order;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        ViewBag.Query = query;
        ViewBag.PageSize = pageSize;

        return View(pagedClients);
    }
}
