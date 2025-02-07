using System.Text.Json;
using ConverterAPI.Models;
using StackExchange.Redis;

namespace ConverterAPI.Services;

public class SessionService(IConnectionMultiplexer redis)
{
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly TimeSpan _sessionLifeTime = TimeSpan.FromHours(24); // Sessions TTL

    public async Task<List<Session>> GetSessionsAsync()
    {
        var keys = await _db.SetMembersAsync("sessions"); // Get all keys

        if (keys.Length == 0)
        {
            return [];
        }
        
        var redisKeys = keys.Select(k => (RedisKey)k.ToString()).ToArray();
        
        var values = await _db.StringGetAsync(redisKeys);
        return values.Where(v => !v.IsNullOrEmpty)
            .Select(v => JsonSerializer.Deserialize<Session>(v!))
            .Where(s => s != null)
            .ToList()!;
    }

    public async Task<Session?> GetSessionByUserIdAsync(int userId)
    {
        var sessionKey = $"session:{userId}";
        string? sessionData = await _db.StringGetAsync(sessionKey);
        
        return sessionData != null ? JsonSerializer.Deserialize<Session>(sessionData) : null;
    }
    
    public async Task CreateSessionAsync(Session session)
    {
        var sessionKey = $"session:{session.userid}";
        var sessionData = JsonSerializer.Serialize(session);
        
        await _db.StringSetAsync(sessionKey, sessionData, _sessionLifeTime);
        await _db.SetAddAsync("sessions", sessionKey); // Add key in set for fast GetSessions
        await _db.StringSetAsync($"session_active:{session.userid}", "true"); // Add key for session restore
    }

    public async Task UpdateSessionAsync(int userId, Action<Session> updateAction)
    {
        var sessionKey = $"session:{userId}";
        var sessionActiveKey = $"session_active:{userId}";
        
        var sessionData = await _db.StringGetAsync(sessionKey);
        if (sessionData.IsNullOrEmpty)
        {
            return;
        }
        
        var session = JsonSerializer.Deserialize<Session>(sessionData!);
        if (session == null)
        {
            return;
        }
        
        var oldActive = session.active;
        var ttl = await _db.KeyTimeToLiveAsync(sessionKey);
        
        updateAction(session);
        
        await _db.StringSetAsync(sessionKey, JsonSerializer.Serialize(session));
        
        if (ttl.HasValue)
        {
            await _db.KeyExpireAsync(sessionKey, ttl);
        }
        
        if (oldActive != session.active)
        {
            if (session.active)
            {
                await _db.StringSetAsync(sessionActiveKey, "true");
            }
            else if (!session.active)
            {
                await _db.KeyDeleteAsync(sessionActiveKey);
            }
        }
    }

    public async Task DeleteSessionAsync(int userId)
    {
        var sessionKey = $"session:{userId}";
        var sessionActiveKey = $"session_active:{userId}";
        
        await _db.KeyDeleteAsync(sessionKey);
        await _db.SetRemoveAsync("sessions", sessionKey);
        await _db.KeyDeleteAsync(sessionActiveKey);
    }
    
    public async Task WatchSessionExpiry()
    {
        var subscriber = redis.GetSubscriber();
        
        await subscriber.SubscribeAsync("__keyevent@0__:expired", async void (channel, key) =>
        {
            var keyString = key.ToString();

            if (keyString.StartsWith("session:"))
            {
                var userId = int.Parse(keyString.Split(':')[1]);
                var sessionActiveKey = $"session_active:{userId}";
    
                var isActive = await _db.StringGetAsync(sessionActiveKey);
                if (isActive.HasValue && isActive.ToString() == "true")
                {
                    var newSession = new Session
                    {
                        userid = userId,
                        datetime = DateTime.UtcNow,
                        amount = 5,
                        active = true
                    };

                    await CreateSessionAsync(newSession);
                }
            }
        });
    }
}