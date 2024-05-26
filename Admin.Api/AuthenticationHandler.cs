using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Api;

public sealed class LoginRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}

public static class AuthenticationHandler
{
    public static void MapAuthentication(IEndpointRouteBuilder router)
    {
        router.MapPost("/login", Login);
        router.MapPost("/logout", Logout);
        router.MapGet("/session", GetSession).RequireAuthorization();
    }

    private static async Task<IResult> Login([FromBody] LoginRequest req, HttpContext ctx)
    {
        if (req is not { Username: "test@mail.ru", Password: "123" })
        {
            return Results.BadRequest();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Test Testovich"),
            new(ClaimTypes.Role, "admin")
        };

        var claimsIdentity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        await ctx.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1),
            }
        );

        return Results.Ok();
    }

    private static async Task<IResult> Logout(HttpContext ctx)
    {
        await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Ok();
    }

    private static async Task<IResult> GetSession(HttpContext ctx)
    {
        var name = ctx.User.FindFirstValue(ClaimTypes.Name);

        if (name is null)
        {
            return Results.Unauthorized();
        }

        return Results.Json(new { name });
    }
}
