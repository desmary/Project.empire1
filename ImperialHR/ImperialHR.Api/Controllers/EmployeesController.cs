// Controllers/EmployeesController.cs
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

    // GET /api/Employees/me
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var me = await _db.Employees.FirstOrDefaultAsync(e => e.Id == CurrentUserId);
        if (me == null) return NotFound();

        return Ok(new { me.Id, me.FullName, me.Email, Role = me.Role.ToString(), me.ManagerId });
    }

    // GET /api/Employees
    [HttpGet]
    [Authorize(Roles = "Emperor,Lord")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Employees
            .Select(e => new
            {
                e.Id,
                e.FullName,
                e.Email,
                Role = e.Role.ToString(),
                e.ManagerId
            })
            .OrderBy(e => e.Id)
            .ToListAsync();

        return Ok(list);
    }

    public record CreateEmployeeDto(
        string FullName,
        string Email,
        string Password,
        string Role,
        int? ManagerId
    );

    // POST /api/Employees
    [HttpPost]
    [Authorize(Roles = "Emperor")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {
        if (!Enum.TryParse<Role>(dto.Role, true, out var role))
            return BadRequest("Role must be Emperor/Lord/Officer");

        var normalizedEmail = (dto.Email ?? "").Trim().ToLowerInvariant();

        if (await _db.Employees.AnyAsync(e => e.Email.ToLower() == normalizedEmail))
            return BadRequest("Email already exists");

        var emp = new Employee
        {
            FullName = (dto.FullName ?? "").Trim(),
            Email = normalizedEmail,
            Role = role,
            ManagerId = dto.ManagerId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            emp.Id,
            emp.FullName,
            emp.Email,
            Role = emp.Role.ToString(),
            emp.ManagerId
        });
    }

    // DELETE /api/Employees/{id}
    // ✅ тільки Emperor може видаляти працівників
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Emperor")]
    public async Task<IActionResult> Delete(int id)
    {
        var emp = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id);
        if (emp == null) return NotFound();

        // Не даємо видалити самого себе (щоб не “вбити” доступ)
        if (id == CurrentUserId)
            return BadRequest("You cannot delete yourself.");

        // Якщо є звʼязки по FK — чистимо заявки (автор/апрувер)
        var relatedRequests = await _db.Requests
            .Where(r => r.EmployeeId == id || r.ApproverId == id)
            .ToListAsync();

        if (relatedRequests.Count > 0)
            _db.Requests.RemoveRange(relatedRequests);

        // Відʼєднуємо підлеглих (щоб не було FK на ManagerId)
        var subs = await _db.Employees.Where(e => e.ManagerId == id).ToListAsync();
        foreach (var s in subs)
            s.ManagerId = null;

        _db.Employees.Remove(emp);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Employee deleted", id });
    }
}
