// Controllers/AuthController.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ImperialHR.Api.Data;
using ImperialHR.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ImperialHR.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ImperialHrDbContext _db;
    private readonly IConfiguration _cfg;

    public AuthController(ImperialHrDbContext db, IConfiguration cfg)
    {
        _db = db;
        _cfg = cfg;
    }

    public record LoginDto(string Email, string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var email = (dto.Email ?? "").Trim().ToLowerInvariant();

        var user = await _db.Employees.FirstOrDefaultAsync(x => x.Email.ToLower() == email);
        if (user is null)
            return Unauthorized("Invalid credentials.");

        var ok = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!ok)
            return Unauthorized("Invalid credentials.");

        var token = CreateJwt(user);

        return Ok(new
        {
            token,
            user = new
            {
                id = user.Id,
                name = user.FullName,
                email = user.Email,
                role = user.Role.ToString()
            }
        });
    }

    
    [HttpPost("seed")]
    public async Task<IActionResult> Seed()
    {
        if (await _db.Employees.AnyAsync())
            return Ok("Seed skipped: Employees already exist.");

        var emperor = new Employee
        {
            FullName = "Emperor",
            Email = "emperor@imperial.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = Role.Emperor,
            ManagerId = null
        };

        var lord = new Employee
        {
            FullName = "Lord",
            Email = "lord@imperial.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = Role.Lord,
            Manager = emperor
        };

        var trooper = new Employee
        {
            FullName = "TK-421",
            Email = "trooper@imperial.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = Role.Officer,
            Manager = lord
        };

        _db.Employees.AddRange(emperor, lord, trooper);
        await _db.SaveChangesAsync();

        return Ok("Seed done. Users: emperor/lord/trooper (password: 123456).");
    }

    // ✅ Альтернатива “не видаляючи БД”: очищає таблиці і сідає заново
    // POST /api/Auth/reset
    [HttpPost("reset")]
    public async Task<IActionResult> Reset()
    {
        // ВАЖЛИВО: спочатку Requests (FK), потім Employees
        // Якщо таблиць Requests ще нема — ця команда впаде, тоді скажи мені текст помилки.
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM Requests");
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM Employees");

        // Скидаємо Identity, щоб Id знову починались з 1
        await _db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Employees', RESEED, 0)");
        await _db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Requests', RESEED, 0)");

        var emperor = new Employee
        {
            FullName = "Emperor",
            Email = "emperor@imperial.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = Role.Emperor,
            ManagerId = null
        };

        var lord = new Employee
        {
            FullName = "Lord",
            Email = "lord@imperial.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = Role.Lord,
            Manager = emperor
        };

        var trooper = new Employee
        {
            FullName = "TK-421",
            Email = "trooper@imperial.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = Role.Officer,
            Manager = lord
        };

        _db.Employees.AddRange(emperor, lord, trooper);
        await _db.SaveChangesAsync();

        return Ok("Reset done. Users: emperor/lord/trooper (password: 123456).");
    }

    private string CreateJwt(Employee user)
    {
        var key = _cfg["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("Jwt:Key is missing in appsettings.json");

        var issuer = _cfg["Jwt:Issuer"] ?? "ImperialHR";
        var audience = _cfg["Jwt:Audience"] ?? "ImperialHR";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(6),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
