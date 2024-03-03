using UrlShortener.Api.Models;

namespace UrlShortener.Api.Services;

public interface IShortener
{
    string Shorten(ShortUrl url);
    string Expand(string code);
    
    void Delete(string code);
}