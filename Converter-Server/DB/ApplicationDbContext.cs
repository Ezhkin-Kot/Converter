namespace ConverterAPI.DB;

using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User.User> Users { get; set; }
    public DbSet<Session.Session> Sessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User.User>().ToTable("users");
        modelBuilder.Entity<Session.Session>().ToTable("sessions");
    }
}