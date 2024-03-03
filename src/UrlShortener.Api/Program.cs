using System.Net;

using UrlShortener.Api.Models;
using UrlShortener.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IShortener, Shortener>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/shorten", (IShortener shorten, string url, DateTime? expiryDate) =>
    {
        var token = shorten.Shorten(new ShortUrl(url, expiryDate, DateTime.UtcNow));
        return "https://fvl.uk/" + token;
    })
    .WithName("shorten")
    .WithOpenApi();

app.MapFallback(RedirectDelegate);

app.Run();

static Task RedirectDelegate(HttpContext httpContext, IShortener shorten)
{
    try
    {
        var token = httpContext.Request.Path.Value?[1..];
        var url = shorten.Expand(token!);
        httpContext.Response.Redirect(url ?? "https://www.fvl.co.uk");
    }
    catch (Exception e)
    {
        httpContext.Response.StatusCode = (int) HttpStatusCode.NotFound;
    }

    return Task.CompletedTask;
}