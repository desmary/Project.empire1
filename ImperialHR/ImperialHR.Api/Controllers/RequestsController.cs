using ImperialHR.Api.Data;
using ImperialHR.Api.Dtos;
using ImperialHR.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ImperialHR.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateRequestDto dto)
        {
            var myId = GetMyId();

            // беремо себе разом з менеджером
            var me = await _db.Employees.FirstOrDefaultAsync(e => e.Id == myId);
            if (me == null) return Unauthorized("Employee not found");

            if (me.ManagerId == null) return BadRequest("You have no manager (lord) assigned");

            // валідація дат
            if (dto.From >= dto.To) return BadRequest("From must be before To");

            var req = new Request
            {
                EmployeeId = myId,
                ApproverId = me.ManagerId.Value,
                Type = (RequestType)dto.Type,
                From = dto.From,
                To = dto.To,
                Status = RequestStatus.Pending
            };

            _db.Requests.Add(req);
            await _db.SaveChangesAsync();

            return Ok(req);
        }

        // GET /api/Requests/my
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> My()
        {
            var myId = GetMyId();

            var list = await _db.Requests
                .Include(r => r.Employee)
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
        [HttpPost("{id}/lord-decision")]
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

            if (!dto.Approve)
            {
                req.Status = RequestStatus.RejectedByLord;
                await _db.SaveChangesAsync();
                return Ok(req);
            }

            // approve by lord -> send to emperor
            req.Status = RequestStatus.ApprovedByLord;

            // emperor is employee with Role == Emperor (припускаємо 1 імператор)
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
        [HttpPost("{id}/emperor-decision")]
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

            if (!dto.Approve)
            {
                req.Status = RequestStatus.RejectedByEmperor;
                await _db.SaveChangesAsync();
                return Ok(req);
            }

            // approve
            req.Status = RequestStatus.ApprovedByEmperor;

            // якщо імператор ввів фінальні дати — замінюємо
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
    }
}
