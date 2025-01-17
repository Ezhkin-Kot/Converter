namespace ConverterAPI.DB.Session;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ConverterAPI.DB.User;

public static class SessionDb
{
    private static readonly string ConnectionString = ConfigurationHelper.GetConnectionString("DefaultConnection");
    
    // Connection to DB
    private static readonly DbContextOptions<ApplicationDbContext> Options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseNpgsql(ConnectionString)
        .Options;

    public static async Task<List<Session>> GetSessions()
    {
        await using var context = new ApplicationDbContext(Options);
        return await context.Sessions.ToListAsync();
    }
    public static async Task<Session?> GetSessionByUserId(int userid)
    {
        await using var context = new ApplicationDbContext(Options);
        return await context.Sessions.OrderBy(s => s.sessionid).LastOrDefaultAsync(s => s.userid == userid);
    }
    
    public static async Task<JsonResult> AuthUser(NewUser? newUser)
    {
        if (newUser != null)
        {
            await using var context = new ApplicationDbContext(Options);
            var user = await context.Users.FirstOrDefaultAsync(u => u.login == newUser.login);
            if (user != null && PasswordManager.VerifyPassword(newUser.password, user.password, user.salt))
            {
                var session = await context.Sessions.OrderBy(s => s.sessionid).LastOrDefaultAsync(s => s.userid == user.id);
                if (session is { active: true })
                {
                    return new JsonResult(new { success = true, user });
                }
                else
                {
                    session = new Session();
                    session.userid = user.id;
                    session.datetime = DateTime.UtcNow;
                    session.amount = 5;
                    session.active = true;
            
                    await context.Sessions.AddAsync(session);
                    await context.SaveChangesAsync();

                    return new JsonResult(new { success = true, user });
                }
            }
            else return new JsonResult(new { success = false, error = "Invalid login or password" });
        }
        else return new JsonResult(new { success = false, error = "Incorrect query"});
    }
    
    public static async Task<JsonResult> UpdAmount(int userid)
    {
        await using var context = new ApplicationDbContext(Options);
        var user = await context.Users.FirstOrDefaultAsync(u => u.id == userid);
        var session = await context.Sessions.OrderBy(s => s.sessionid).LastOrDefaultAsync(s => s.userid == userid);

        if (session != null && user != null)
        {
            if (session.active)
            {
                if (user.premium)
                {
                    session.amount = 5;
                    return new JsonResult(new { session.amount, message = "Amount unlimited" });
                }
                if (session.amount <= 0)
                {
                    return new JsonResult(new { session.amount, message = "Amount expire" });
                }
                
                session.amount--;
                context.Sessions.Update(session);
                await context.SaveChangesAsync();
                
                return new JsonResult(new { session.amount, message = "Amount updated" });
            }
            else return new JsonResult(new { success = false, error = "Session isn't active" });
        }
        else return new JsonResult(new { success = false, error = "Session not found" });
    }
    
    public static async Task<JsonResult> CloseSession(int userid)
    {
        await using var context = new ApplicationDbContext(Options);
        var session = await context.Sessions.OrderBy(s => s.sessionid).LastOrDefaultAsync(s => s.userid == userid);

        if (session != null)
        {
            if (session.active)
            {
                session.active = false;
                context.Sessions.Update(session);
                await context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Session closed" });
            }
            else return new JsonResult(new { success = false, error = "Session is already closed" });
        }
        else return new JsonResult(new { success = false, error = "Session not found"});
    }
    
    public static async Task<JsonResult> DeleteSession(int userid)
    {
        await using var context = new ApplicationDbContext(Options);
        var session = await context.Sessions.FirstOrDefaultAsync(s => s.userid == userid);
        if (session == null)
        {
            return new JsonResult(new {success = false, error = "Session not found" });
        }

        context.Sessions.Remove(session);
        await context.SaveChangesAsync();

        return new JsonResult(new { success = true, message = "Session successfully deleted"});
    }
}