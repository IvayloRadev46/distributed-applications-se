using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
namespace TermProject.Models;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CarsController : ControllerBase
{
    private readonly ProjectDbContext _context;
    public CarsController(ProjectDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        string? vin, string? licensePlate,
        int page = 1, int pageSize = 10,
        string? sort = "model")
    {
        var query = _context.Cars.Include(c => c.Client).AsQueryable();

        if (!string.IsNullOrEmpty(vin))
            query = query.Where(c => c.VIN.Contains(vin));

        if (!string.IsNullOrEmpty(licensePlate))
            query = query.Where(c => c.LicensePlate.Contains(licensePlate));

        query = sort switch
        {
            "year" => query.OrderBy(c => c.ManufactureYear),
            _ => query.OrderBy(c => c.Model)
        };

        var cars = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(cars);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Car car)
    {
        _context.Cars.Add(car);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = car.Id }, car);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var car = await _context.Cars.Include(c => c.Client).FirstOrDefaultAsync(c => c.Id == id);
        return car == null ? NotFound() : Ok(car);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Car updatedCar)
    {
        if (id != updatedCar.Id) return BadRequest();

        _context.Entry(updatedCar).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var car = await _context.Cars.FindAsync(id);
        if (car == null) return NotFound();

        _context.Cars.Remove(car);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
