namespace ConverterAPI.DB.User;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ConverterAPI.DB.Session;

public static class UserDb
{
    private static readonly string ConnectionString = ConfigurationHelper.GetConnectionString("DefaultConnection");

    // Connection to DB
    private static readonly DbContextOptions<ApplicationDbContext> Options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseNpgsql(ConnectionString)
        .Options;

    public static async Task<JsonResult> GetUsers()
    {
        await using var context = new ApplicationDbContext(Options);
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
            return new JsonResult(new { message = "There are no users." });
        }
        
        return new JsonResult( new { users = publicUsers} );
    }

    public static async Task<JsonResult> GetUserById(int id)
    {
        await using var context = new ApplicationDbContext(Options);
        var user = await context.Users.FirstOrDefaultAsync(u => u.id == id);
        if (user == null)
        {
            return new JsonResult(new { success = false, message = "User not found." });
        }

        PublicUser publicUser = new PublicUser();
        publicUser.id = user.id;
        publicUser.login = user.login;
        publicUser.premium = user.premium;
        
        return new JsonResult(new { success = true, user = publicUser });
    }

    public static async Task<JsonResult> CreateUser(NewUser? newUser)
    {
        if (newUser != null)
        {
            await using var context = new ApplicationDbContext(Options);
            var createdUser = new User();
            createdUser.login = newUser.login;
            (createdUser.password, createdUser.salt) = PasswordManager.HashPassword(newUser.password);
            createdUser.premium = false;
            
            var existUser = await context.Users.FirstOrDefaultAsync(u => u.login == createdUser.login);
            if (existUser != null)
            {
                return new JsonResult(new { message = "User already exists" });
            }
            
            await context.Users.AddAsync(createdUser);
            await context.SaveChangesAsync();
            
            var user = await context.Users.FirstOrDefaultAsync(u => u.login == createdUser.login);
            return new JsonResult(new { success = true, user });
        }
        else return new JsonResult(new { success = false, error = "Incorrect query" });
    }

    public static async Task<JsonResult> UpdateUser(User? updatedUser)
    {
        if (updatedUser != null)
        {
            await using var context = new ApplicationDbContext(Options);
            var user = await context.Users.FirstOrDefaultAsync(u => u.id == updatedUser.id);

            if (user != null)
            {
                user.login = updatedUser.login ?? user.login;
                (user.password, user.salt) = (updatedUser.password != null) ? 
                    PasswordManager.HashPassword(updatedUser.password) : (user.password, user.salt);
                user.premium = updatedUser.premium;

                context.Users.Update(user);
                await context.SaveChangesAsync();

                return new JsonResult("User updated");
            }
            else return new JsonResult("User not found");
        }
        else return new JsonResult("Incorrect query");
    }
    
    public static async Task<JsonResult> ChangePremium(int id, bool premium)
    {
        await using var context = new ApplicationDbContext(Options);
        var user = await context.Users.FirstOrDefaultAsync(u => u.id == id);

        if (user != null)
        {
            user.premium = premium;

            context.Users.Update(user);
            await context.SaveChangesAsync();

            return new JsonResult(new { success = true, user.premium });
        }
        else return new JsonResult("User not found");
    }

    public static async Task<JsonResult> DeleteUser(int id)
    {
        await using var context = new ApplicationDbContext(Options);
        var user = await context.Users.FindAsync(id);
        if (user == null)
        {
            return new JsonResult("User not found");
        }
        
        await SessionDb.DeleteSession(id);
        context.Users.Remove(user);
        await context.SaveChangesAsync();

        return new JsonResult("User successfully deleted");
    }
}