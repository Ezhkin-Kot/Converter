using ConverterAPI.Models;
using ConverterAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConverterAPI.Controllers;

[ApiController]
[Route("users")]
public class UsersController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var publicUsers = await context.Users
            .Select(user => new PublicUser
            {
                id = user.id,
                login = user.login,
                premium = user.premium
            })
            .ToListAsync();
        
        if (publicUsers.Count == 0)
        {
            return NotFound(new { message = "There are no users." });
        }
        
        return Ok(new { users = publicUsers });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.id == id);
        
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var publicUser = new PublicUser
        {
            id = user.id,
            login = user.login,
            premium = user.premium
        };

        return Ok(new { user = publicUser });
    }

    [HttpPost("reg")]
    public async Task<IActionResult> CreateUser([FromBody] NewUser? newUser)
    {
        if (newUser?.login == null || newUser.password == null)
        {
            return BadRequest(new { message = "Invalid request." });
        }
        
        var createdUser = new User
        {
            login = newUser.login,
            premium = false
        };
        (createdUser.password, createdUser.salt) = PasswordManager.HashPassword(newUser.password);
        
        var existUser = await context.Users.FirstOrDefaultAsync(u => u.login == createdUser.login);
        
        if (existUser != null)
        {
            return Conflict(new { message = "User already exists." });
        }
        
        await context.Users.AddAsync(createdUser);
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetUserById), new { createdUser.id }, 
            new { message = $"User {createdUser.login} successfully created." });
    }
    
    [HttpPatch]
    public async Task<IActionResult> UpdateUser([FromBody] UpdatedUser? updatedUser)
    {
        if (updatedUser?.currentLogin == null || updatedUser.currentPassword == null)
        {
            return BadRequest(new { message = "Invalid request." });
        }
        
        var user = await context.Users.FirstOrDefaultAsync(u => u.login == updatedUser.currentLogin);

        if (user == null)
        {
            // Fake password verification for security
            PasswordManager.VerifyPassword(updatedUser.currentPassword, updatedUser.currentLogin, "s+abobaAboBA9+0wUpvS0g==");
            return Unauthorized(new { message = "Invalid login or password." });
        }
        if (!PasswordManager.VerifyPassword(updatedUser.currentPassword, user.password, user.salt))
        {
            return Unauthorized(new { message = "Invalid login or password." });
        }
        
        user.login = updatedUser.newLogin ?? user.login;
        (user.password, user.salt) = (updatedUser.newPassword != null) ? 
            PasswordManager.HashPassword(updatedUser.newPassword) : (user.password, user.salt);
        user.premium = updatedUser.premium;
        
        context.Users.Update(user);
        await context.SaveChangesAsync();

        return Ok(new { message = $"User {user.login} successfully updated." });
    }
    
    [HttpPatch("prem/{id:int}/{premium:bool}")]
    public async Task<IActionResult> ChangePremium(int id, bool premium)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.id == id);

        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }
        
        user.premium = premium;

        context.Users.Update(user);
        await context.SaveChangesAsync();

        return Ok(new { user.premium, message = user.premium ? 
            $"Premium of user {user.login} activated." : $"Premium of user {user.login} deactivated." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await context.Users.FindAsync(id);
        
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }
        
        context.Users.Remove(user);
        await context.SaveChangesAsync();

        return Ok(new { message = $"User {user.login} successfully deleted." });
    }
}