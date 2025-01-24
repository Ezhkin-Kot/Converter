using Microsoft.EntityFrameworkCore;

namespace ConverterAPI.Models;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Models.User> Users { get; set; }
    public DbSet<Models.Session> Sessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.User>().ToTable("users");
        modelBuilder.Entity<Models.Session>().ToTable("sessions");
    }
}