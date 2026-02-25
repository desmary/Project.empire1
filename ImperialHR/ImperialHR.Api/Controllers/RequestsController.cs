using ImperialHR.Api.Data;
using ImperialHR.Api.Dtos;
using ImperialHR.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ImperialHR.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RequestsController : ControllerBase
{
    private readonly ImperialHrDbContext _db;

    public RequestsController(ImperialHrDbContext db)
    {
        _db = db;
    }

    private int GetMyId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(idStr!);
    }

    // POST /api/Requests
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRequestDto dto)
    {
        var myId = GetMyId();

        var me = await _db.Employees.FirstOrDefaultAsync(e => e.Id == myId);
        if (me == null) return Unauthorized("Employee not found");

        if (me.ManagerId == null) return BadRequest("You have no manager (lord) assigned");
        if (dto.From >= dto.To) return BadRequest("From must be before To");

        RequestType type;
        try
        {
            type = MapRequestType(dto.Type);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        var req = new Request
        {
            EmployeeId = myId,
            ApproverId = me.ManagerId.Value,
            Type = type,
            From = dto.From,
            To = dto.To,
            Comment = dto.Comment,
            Status = RequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Requests.Add(req);
        await _db.SaveChangesAsync();

        return Ok(req);
    }

    // GET /api/Requests/my
    [HttpGet("my")]
    public async Task<IActionResult> My()
    {
        var myId = GetMyId();

        var list = await _db.Requests
            .Include(r => r.Employee)
            .Include(r => r.Approver)
            .OrderByDescending(r => r.Id)
            .Where(r => r.EmployeeId == myId)
            .ToListAsync();

        return Ok(list);
    }

    // GET /api/Requests/lord/inbox
    [HttpGet("lord/inbox")]
    [Authorize(Roles = "Lord")]
    public async Task<IActionResult> LordInbox()
    {
        var myId = GetMyId();

        var list = await _db.Requests
            .Include(r => r.Employee)
            .OrderByDescending(r => r.Id)
            .Where(r => r.ApproverId == myId && r.Status == RequestStatus.Pending)
            .ToListAsync();

        return Ok(list);
    }

    // POST /api/Requests/{id}/lord-decision
    [HttpPost("{id:int}/lord-decision")]
    [Authorize(Roles = "Lord")]
    public async Task<IActionResult> LordDecision(int id, [FromBody] DecisionDto dto)
    {
        var myId = GetMyId();

        var req = await _db.Requests
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (req == null) return NotFound();
        if (req.ApproverId != myId) return Forbid();
        if (req.Status != RequestStatus.Pending) return BadRequest("Request is not Pending");

        req.UpdatedAt = DateTime.UtcNow;

        if (!dto.Approve)
        {
            req.Status = RequestStatus.RejectedByLord;
            await _db.SaveChangesAsync();
            return Ok(req);
        }

        req.Status = RequestStatus.ApprovedByLord;

        var emperor = await _db.Employees.FirstOrDefaultAsync(e => e.Role == Role.Emperor);
        if (emperor == null) return StatusCode(500, "Emperor not found");

        req.ApproverId = emperor.Id;

        await _db.SaveChangesAsync();
        return Ok(req);
    }

    // GET /api/Requests/emperor/inbox
    [HttpGet("emperor/inbox")]
    [Authorize(Roles = "Emperor")]
    public async Task<IActionResult> EmperorInbox()
    {
        var myId = GetMyId();

        var list = await _db.Requests
            .Include(r => r.Employee)
            .OrderByDescending(r => r.Id)
            .Where(r => r.ApproverId == myId && r.Status == RequestStatus.ApprovedByLord)
            .ToListAsync();

        return Ok(list);
    }

    // POST /api/Requests/{id}/emperor-decision
    [HttpPost("{id:int}/emperor-decision")]
    [Authorize(Roles = "Emperor")]
    public async Task<IActionResult> EmperorDecision(int id, [FromBody] EmperorDecisionDto dto)
    {
        var myId = GetMyId();

        var req = await _db.Requests
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (req == null) return NotFound();
        if (req.ApproverId != myId) return Forbid();
        if (req.Status != RequestStatus.ApprovedByLord) return BadRequest("Request is not ApprovedByLord");

        req.UpdatedAt = DateTime.UtcNow;

        if (!dto.Approve)
        {
            req.Status = RequestStatus.RejectedByEmperor;
            await _db.SaveChangesAsync();
            return Ok(req);
        }

        req.Status = RequestStatus.ApprovedByEmperor;

        if (dto.FinalFrom.HasValue && dto.FinalTo.HasValue)
        {
            if (dto.FinalFrom.Value >= dto.FinalTo.Value)
                return BadRequest("FinalTo must be after FinalFrom");

            req.From = dto.FinalFrom.Value;
            req.To = dto.FinalTo.Value;
        }

        await _db.SaveChangesAsync();
        return Ok(req);
    }

    // DELETE /api/Requests/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var myId = GetMyId();

        var req = await _db.Requests.FirstOrDefaultAsync(r => r.Id == id);
        if (req == null) return NotFound();

        var isEmperor = User.IsInRole("Emperor");

        if (!isEmperor)
        {
            if (req.EmployeeId != myId) return Forbid();
            if (req.Status != RequestStatus.Pending)
                return BadRequest("You can delete only Pending requests.");
        }

        _db.Requests.Remove(req);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Request deleted", id });
    }

    private static RequestType MapRequestType(string? type)
    {
        var t = (type ?? "").Trim().ToLowerInvariant();

        return t switch
        {
            "annual leave" => RequestType.AnnualLeave,
            "sick leave" => RequestType.SickLeave,
            "unpaid leave" => RequestType.UnpaidLeave,
            "study leave" => RequestType.StudyLeave,
            _ => throw new ArgumentException("Type must be one of: Annual leave, Sick leave, Unpaid leave, Study leave")
        };
    }
}