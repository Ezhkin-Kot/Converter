using ConverterAPI.Models;
using ConverterAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConverterAPI.Controllers;

[ApiController]
[Route("sessions")]
public class SessionsController(ApplicationDbContext postgresDbContext, SessionService redisSessionService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSessions()
    {
        var sessions = await redisSessionService.GetSessionsAsync();

        if (sessions.Count == 0)
        {
            return NotFound(new { message = "No sessions found." });
        }
        
        return Ok(new { sessions });
    }
    
    [HttpGet("{userid:int}")]
    public async Task<IActionResult> GetSessionByUserId(int userid)
    {
        var session = await redisSessionService.GetSessionByUserIdAsync(userid);
        
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
        
        var user = await postgresDbContext.Users.FirstOrDefaultAsync(u => u.login == newUser.login);

        if (user == null || !PasswordManager.VerifyPassword(newUser.password, user.password, user.salt))
        {
            return Unauthorized(new { message = "Invalid login or password" });
        }
        
        var session = await redisSessionService.GetSessionByUserIdAsync(user.id);
        
        var publicUser = new PublicUser
        {
            id = user.id,
            login = user.login,
            premium = user.premium
        };
        
        if (session != null)
        {
            await redisSessionService.UpdateSessionAsync(user.id, s => s.active = true);
            return Ok(new { session, user = publicUser, message = "Connected to existing session" });
        }
        
        session = new Session
        {
            userid = user.id,
            datetime = DateTime.UtcNow,
            amount = 5,
            active = true
        };

        await redisSessionService.CreateSessionAsync(session);

        return Ok(new { session, user = publicUser, message = "New session started" });
    }
    
    [HttpPatch("upd/{userid:int}")]
    public async Task<IActionResult> UpdAmount(int userid)
    {
        var user = await postgresDbContext.Users.FirstOrDefaultAsync(u => u.id == userid);
        var session = await redisSessionService.GetSessionByUserIdAsync(userid);

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
            await redisSessionService.UpdateSessionAsync(user.id, s => s.amount = 5);
            return Ok(new { session.amount, message = "Amount unlimited." });
        }
        
        if (session.amount <= 0)
        {
            return StatusCode(403, new { message = "Amount expired." });
        }

        await redisSessionService.UpdateSessionAsync(user.id, s => s.amount--);
        session.amount--;

        return Ok(new { session.amount, message = "Amount updated" });
    }
    
    [HttpPatch("close/{userid:int}")]
    public async Task<IActionResult> CloseSession(int userid)
    {
        var session = await redisSessionService.GetSessionByUserIdAsync(userid);

        if (session == null)
        {
            return NotFound(new { message = "Session does not exist." });
        }
        if (!session.active)
        {
            return BadRequest(new { message = "Session is already closed." });
        }
        
        await redisSessionService.UpdateSessionAsync(userid, s => s.active = false);

        return Ok(new { message = "Session closed." });
    }
    
    [HttpDelete("{userid:int}")]
    public async Task<IActionResult> DeleteSession(int userid)
    {
        var session = await redisSessionService.GetSessionByUserIdAsync(userid);
        
        if (session == null)
        {
            return NotFound(new { message = "No session found." });
        }

        await redisSessionService.DeleteSessionAsync(userid);

        return Ok(new { message = "Session successfully deleted." });
    }
}