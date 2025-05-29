using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using Garage.WebClient.Models;

public class DashboardController : Controller
{
    private readonly HttpClient _client;
    private readonly string _baseUrl = "https://localhost:7200/api";

    public DashboardController(IHttpClientFactory factory)
    {
        _client = factory.CreateClient();
    }

    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString("token");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Index", "Login");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var clients = await GetListAsync<ClientModel>("Clients");
        var cars = await GetListAsync<CarModel>("Cars");
        var repairs = await GetListAsync<RepairModel>("Repairs");

       
        foreach (var car in cars)
        {
            var client = clients.FirstOrDefault(c => c.Id == car.ClientId);
            car.ClientFullName = client != null ? $"{client.FirstName} {client.LastName}" : "Неизвестен клиент";
        }

       
        foreach (var repair in repairs)
        {
            var car = cars.FirstOrDefault(c => c.Id == repair.CarId);
            repair.CarModel = car != null ? $"{car.Model} ({car.LicensePlate})" : "Непозната кола";
        }

        ViewBag.ClientCount = clients.Count;
        ViewBag.CarCount = cars.Count;
        ViewBag.RepairCount = repairs.Count(r => r.Status != "Completed");

        ViewBag.CarList = cars;

        ViewBag.RecentRepairs = repairs
            .OrderByDescending(r => r.StartDate)
            .Take(5)
            .ToList();

        ViewBag.RepairStatusStats = repairs
            .GroupBy(r => r.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        return View();
    }

    private async Task<List<T>> GetListAsync<T>(string endpoint)
    {
        var response = await _client.GetAsync($"{_baseUrl}/{endpoint}");
        if (!response.IsSuccessStatusCode)
            return new List<T>();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return await response.Content.ReadFromJsonAsync<List<T>>(options) ?? new List<T>();
    }
}
