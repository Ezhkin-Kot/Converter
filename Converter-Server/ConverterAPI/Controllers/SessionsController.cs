using ConverterAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConverterAPI.Controllers;

[ApiController]
[Route("sessions")]
public class SessionsController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSessions()
    {
        var sessions = await context.Sessions.ToListAsync();

        if (sessions.Count == 0)
        {
            return NotFound(new { message = "No sessions found." });
        }
        
        return Ok(new { sessions });
    }
    
    [HttpGet("{userid:int}")]
    public async Task<IActionResult> GetSessionByUserId(int userid)
    {
        var session = await context.Sessions.OrderBy(s => s.sessionid)
            .LastOrDefaultAsync(s => s.userid == userid);
        
        if (session == null)
        {
            return NotFound(new { message = "No session found." });
        }
        
        return Ok(new { session });
    }
    
    [HttpPost("auth")]
    public async Task<IActionResult> AuthUser([FromBody] NewUser? newUser)
    {
        if (newUser == null)
        {
            return BadRequest(new { message = "Invalid request" });
        }
        
        var user = await context.Users.FirstOrDefaultAsync(u => u.login == newUser.login);

        if (user == null || !PasswordManager.VerifyPassword(newUser.password, user.password, user.salt))
        {
            return new JsonResult(new { success = false, error = "Invalid login or password" });
        }
        
        var session = await context.Sessions.OrderBy(s => s.sessionid)
            .LastOrDefaultAsync(s => s.userid == user.id);
        
        if (session is { active: true })
        {
            return Ok(new { session, message = "Connected to existing session" });
        }
        
        session = new Session
        {
            userid = user.id,
            datetime = DateTime.UtcNow,
            amount = 5,
            active = true
        };

        await context.Sessions.AddAsync(session);
        await context.SaveChangesAsync();

        return Ok(new { session, message = "New session started" });
    }
    
    [HttpPatch("upd/{userid:int}")]
    public async Task<IActionResult> UpdAmount(int userid)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.id == userid);
        var session = await context.Sessions.OrderBy(s => s.sessionid)
            .LastOrDefaultAsync(s => s.userid == userid);

        if (user == null)
        {
            return BadRequest(new { message = "User does not exist." });
        }
        if (session == null)
        {
            return NotFound(new { message = "No session found." });
        }
        if (!session.active)
        {
            return BadRequest(new { message = "Session is not active." });
        }
        
        if (user.premium)
        {
            session.amount = 5;
            return Ok(new { session.amount, message = "Amount unlimited." });
        }
        
        if (session.amount <= 0)
        {
            return StatusCode(403, new { message = "Amount expired." });
        }

        session.amount--;
        context.Sessions.Update(session);
        await context.SaveChangesAsync();

        return Ok(new { session.amount, message = "Amount updated" });
    }
    
    [HttpPatch("close/{userid:int}")]
    public async Task<IActionResult> CloseSession(int userid)
    {
        var session = await context.Sessions.OrderBy(s => s.sessionid)
            .LastOrDefaultAsync(s => s.userid == userid);

        if (session == null)
        {
            return NotFound(new { message = "Session does not exist." });
        }
        if (!session.active)
        {
            return BadRequest(new { message = "Session is already closed." });
        }
        
        session.active = false;
        context.Sessions.Update(session);
        await context.SaveChangesAsync();

        return Ok(new { message = "Session closed." });
    }
    
    [HttpDelete("{userid:int}")]
    public async Task<IActionResult> DeleteSession(int userid)
    {
        var session = await context.Sessions.FirstOrDefaultAsync(s => s.userid == userid);
        
        if (session == null)
        {
            return NotFound(new { message = "No session found." });
        }

        context.Sessions.Remove(session);
        await context.SaveChangesAsync();

        return Ok(new { message = "Session successfully deleted." });
    }
}