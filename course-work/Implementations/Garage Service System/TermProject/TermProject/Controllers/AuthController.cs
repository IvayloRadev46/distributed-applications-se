using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.Models; 

namespace TermProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ProjectDbContext _context;

        public AuthController(IConfiguration config, ProjectDbContext context)
        {
            _config = config;
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User login)
        {
            if (login.Username == "admin" && login.Password == "admin")
            {
                return Ok(GenerateToken("admin@garage.com", "Admin"));
            }

            var client = _context.Clients.FirstOrDefault(c =>
                c.Email == login.Username && c.Password == login.Password);

            if (client != null)
            {
                return Ok(GenerateToken(client.Email, "Client"));
            }

            return Unauthorized("Невалидни данни за вход.");
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] Client model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_context.Clients.Any(c => c.Email == model.Email))
                return BadRequest("Имейлът вече съществува.");

            model.RegistrationDate = DateTime.UtcNow;
            model.LoyaltyPoints = 0;

            _context.Clients.Add(model);
            _context.SaveChanges();

            return Ok("Регистрацията е успешна.");
        }

        private object GenerateToken(string email, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                role
            };
        }
    }
}
