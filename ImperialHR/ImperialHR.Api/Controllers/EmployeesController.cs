using ImperialHR.Api.Data;
using ImperialHR.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ImperialHR.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly ImperialHrDbContext _db;
    public EmployeesController(ImperialHrDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Будь-хто: отримати себе
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var me = await _db.Employees.FirstOrDefaultAsync(e => e.Id == CurrentUserId);
        if (me == null) return NotFound();
        return Ok(new { me.Id, me.FullName, me.Email, Role = me.Role.ToString(), me.ManagerId });
    }

    // Emperor/Lord: список співробітників (для демо, під захист)
    [HttpGet]
    [Authorize(Roles = "Emperor,Lord")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Employees
            .Select(e => new { e.Id, e.FullName, e.Email, Role = e.Role.ToString(), e.ManagerId })
            .OrderBy(e => e.Id)
            .ToListAsync();

        return Ok(list);
    }

    // Emperor: створити працівника (щоб показати CRUD)
    public record CreateEmployeeDto(string FullName, string Email, string Password, string Role, int? ManagerId);

    [HttpPost]
    [Authorize(Roles = "Emperor")]
    public async Task<IActionResult> Create(CreateEmployeeDto dto)
    {
        if (!Enum.TryParse<Role>(dto.Role, true, out var role))
            return BadRequest("Role must be Emperor/Lord/Officer");

        if (await _db.Employees.AnyAsync(e => e.Email == dto.Email))
            return BadRequest("Email already exists");

        var emp = new Employee
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Role = role,
            ManagerId = dto.ManagerId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        return Ok(new { emp.Id, emp.FullName, emp.Email, Role = emp.Role.ToString(), emp.ManagerId });
    }
}
