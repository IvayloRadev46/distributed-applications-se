using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
namespace TermProject.Models;
using Microsoft.EntityFrameworkCore;




[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly ProjectDbContext _context;
    public ClientsController(ProjectDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        string? firstName, string? lastName,
        int page = 1, int pageSize = 10,
        string? sort = "firstName")
    {
        var query = _context.Clients.AsQueryable();

        if (!string.IsNullOrEmpty(firstName))
            query = query.Where(c => c.FirstName.Contains(firstName));

        if (!string.IsNullOrEmpty(lastName))
            query = query.Where(c => c.LastName.Contains(lastName));

        query = sort switch
        {
            "lastName" => query.OrderBy(c => c.LastName),
            _ => query.OrderBy(c => c.FirstName)
        };

        var clients = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(clients);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Client client)
    {
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = client.Id }, client);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        return client == null ? NotFound() : Ok(client);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Client updatedClient)
    {
        if (id != updatedClient.Id) return BadRequest();

        _context.Entry(updatedClient).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null) return NotFound();

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}