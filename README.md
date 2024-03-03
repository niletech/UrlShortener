# URL Shortner

This is a simple URL shortner that uses a simple hash function to generate a short URL from a long URL. The short URL 
is then used to redirect to the original long URL.

## System Requirements

- .NET Core 8.0
- Redis Server

## How to run

1. Clone the repository
2. Run `dotnet restore` to restore the dependencies
3. Run `dotnet run` to start the server
4. The server will start on `http://localhost:5000`
5. You can now use the server to shorten URLs
6. To shorten a URL, make a POST request to `http://localhost:5000/shorten` with the following JSON payload:
    ```json
    {
        "url": "https://www.google.com",
        "expiry": "2021-12-31T23:59:59" // Optional
    }
    ```
    The server will respond with a string payload containing the shortened URL:
    ```text
    https://fvl.uk/6E9cK6
    ```
7. To use the shortened URL, simply make a GET request to the shortened URL and the server will redirect you to the original URL.

## How it works

The server uses a simple hash function to generate a short URL from a long URL. The hash function is a simple MD5 hash 
of the long URL. The server then stores the short URL and the long URL in a Redis database. When a request is made to 
the short URL, the server looks up the long URL in the Redis database and redirects the user to the long URL.

## Expiry

The server also supports URL expiry. When a URL is shortened, the user can specify an expiry date. The server will then 
store the expiry date along with the short URL and long URL in the Redis database. When a request is made to the short 
URL, the server will check if the URL has expired and respond with a 404 if it has.