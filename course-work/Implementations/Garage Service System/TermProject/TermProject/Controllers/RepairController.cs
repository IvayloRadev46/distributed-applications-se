using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TermProject.Models;
using System;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RepairsController : ControllerBase
{
    private readonly ProjectDbContext _context;
    public RepairsController(ProjectDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        string? status, DateTime? startDate,
        int page = 1, int pageSize = 10,
        string? sort = "startDate")
    {
        var query = _context.Repairs.Include(r => r.Car).AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status.Contains(status));

        if (startDate.HasValue)
            query = query.Where(r => r.StartDate.Date == startDate.Value.Date);

        query = sort switch
        {
            "price" => query.OrderBy(r => r.Price),
            _ => query.OrderBy(r => r.StartDate)
        };

        var repairs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(repairs);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Repair repair)
    {
        _context.Repairs.Add(repair);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = repair.Id }, repair);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var repair = await _context.Repairs.Include(r => r.Car).FirstOrDefaultAsync(r => r.Id == id);
        return repair == null ? NotFound() : Ok(repair);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Repair updatedRepair)
    {
        if (id != updatedRepair.Id) return BadRequest();

        _context.Entry(updatedRepair).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var repair = await _context.Repairs.FindAsync(id);
        if (repair == null) return NotFound();

        _context.Repairs.Remove(repair);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
