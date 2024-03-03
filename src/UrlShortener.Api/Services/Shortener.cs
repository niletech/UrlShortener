using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using StackExchange.Redis;

using UrlShortener.Api.Models;

namespace UrlShortener.Api.Services;

public class Shortener : IShortener
{
    private readonly IDatabase _db;

    public Shortener(IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("RedisConnection");
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionString ??
                                                                    throw new InvalidOperationException(
                                                                        "Redis connection string not found in configuration."));
        _db = redis.GetDatabase();
    }

    public string Shorten(ShortUrl url)
    {
        string shortUrl = JsonSerializer.Serialize(url);
        string shortCode = GenerateShortUrl();

        while (Exists(shortCode))
        {
            shortCode = GenerateShortUrl();
        }

        _db.StringSet(shortCode, shortUrl);
        return shortCode;
    }

    public string Expand(string code)
    {
        var retrievedObject = _db.StringGet(code);
        if (retrievedObject.IsNull)
        {
            throw new InvalidOperationException("Invalid short URL");
        }

        var shortUrl = JsonSerializer.Deserialize<ShortUrl>(retrievedObject!);
        if (shortUrl == null)
        {
            throw new InvalidOperationException("Invalid short URL");
        }

        if (shortUrl.ExpiresAt.HasValue && shortUrl.ExpiresAt.Value < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Short URL has expired");
        }

        string updatedShortUrl = JsonSerializer.Serialize(shortUrl with { Clicks = shortUrl.Clicks + 1 });
        _db.StringSet(code, updatedShortUrl);

        return shortUrl.Url;
    }

    public void Delete(string code)
    {
        _db.KeyDelete(code);
    }

    private bool Exists(string code)
    {
        return _db.KeyExists(code);
    }

    private string GenerateShortUrl()
    {
        const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var stringBuilder = new StringBuilder(6);

        // Create a byte array to hold the random bytes.
        byte[] randomNumber = new byte[1];

        using (var rng = RandomNumberGenerator.Create())
        {
            for (int i = 0; i < 6; i++)
            {
                rng.GetBytes(randomNumber);
                // Convert the byte to an integer value to use as an index. 
                // Modulo by the number of allowed characters to ensure the index is within bounds.
                int index = randomNumber[0] % allowedChars.Length;
                stringBuilder.Append(allowedChars[index]);
            }
        }

        return stringBuilder.ToString();
    }
}