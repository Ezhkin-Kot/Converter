using ConverterAPI.Services;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Connection to DBs
var postgresConnectionString = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));

var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddSingleton<SessionService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
            "http://localhost",
            "https://localhost:80",
            "http://client"
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Sessions expiry listener for restoring active sessions
using (var scope = app.Services.CreateScope())
{
    var sessionService = scope.ServiceProvider.GetRequiredService<SessionService>();
    
    await Task.Run(() => sessionService.WatchSessionExpiry());
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("AllowSpecificOrigins");
app.Run();