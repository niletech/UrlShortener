namespace UrlShortener.Api.Models;

public sealed record ShortUrl(string Url, DateTime? ExpiresAt, DateTime CreatedAt, int Clicks = 0);