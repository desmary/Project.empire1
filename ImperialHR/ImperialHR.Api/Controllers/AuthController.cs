using ImperialHR.Api.Data;
using ImperialHR.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ImperialHR.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ImperialHrDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(ImperialHrDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public record LoginDto(string Email, string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _db.Employees.FirstOrDefaultAsync(x => x.Email == dto.Email);
        if (user == null) return Unauthorized("Invalid credentials");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        var token = CreateToken(user);
        return Ok(new { token });
    }

    // Демо-напoвнення: Palpatine / Vader / Trooper (пароль 123456)
    [HttpPost("seed")]
    public async Task<IActionResult> Seed()
    {
        if (await _db.Employees.AnyAsync(e => e.Email == "palpatine@empire.com"))
            return Ok("Already seeded");

        var palpatine = new Employee
        {
            FullName = "Sheev Palpatine",
            Email = "palpatine@empire.com",
            Role = Role.Emperor,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456")
        };

        var vader = new Employee
        {
            FullName = "Darth Vader",
            Email = "vader@empire.com",
            Role = Role.Lord,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456")
        };

        _db.Employees.AddRange(palpatine, vader);
        await _db.SaveChangesAsync();

        var trooper = new Employee
        {
            FullName = "Stormtrooper FN-2187",
            Email = "trooper@empire.com",
            Role = Role.Officer,
            ManagerId = vader.Id,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456")
        };

        _db.Employees.Add(trooper);
        await _db.SaveChangesAsync();

        return Ok("Seeded: palpatine@empire.com / vader@empire.com / trooper@empire.com (password 123456)");
    }

    private string CreateToken(Employee user)
    {
        var key = _config["Jwt:Key"]!;
        var issuer = _config["Jwt:Issuer"]!;
        var audience = _config["Jwt:Audience"]!;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
