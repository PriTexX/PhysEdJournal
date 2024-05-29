using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Infrastructure.Database;

namespace Admin.Api;

public sealed class LoginRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required bool Remember { get; init; }
}

public static class AuthenticationHandler
{
    public static void MapAuthentication(IEndpointRouteBuilder router)
    {
        router.MapPost("/login", Login);
        router.MapPost("/logout", Logout);
        router.MapGet("/session", GetSession).RequireAuthorization();
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest req,
        HttpContext ctx,
        [FromServices] LkAuthClient authClient,
        [FromServices] ApplicationContext dbCtx
    )
    {
        var authResult = await authClient.Authenticate(req.Username, req.Password);

        if (authResult.IsErr)
        {
            return Results.Unauthorized();
        }

        var authData = authResult.UnsafeValue;

        var teacher = await dbCtx.Teachers.FindAsync(authData.PersonGuid);

        if (teacher is null)
        {
            return Results.Unauthorized();
        }

        if (
            !(
                teacher.Permissions.HasFlag(TeacherPermissions.AdminAccess)
                || teacher.Permissions.HasFlag(TeacherPermissions.SuperUser)
            )
        )
        {
            return Results.StatusCode(403);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, authData.FullName),
            new(ClaimTypes.Role, "admin"),
            new("avatar", authData.PictureUrl),
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
                ExpiresUtc = req.Remember
                    ? DateTimeOffset.UtcNow.AddDays(1)
                    : DateTimeOffset.UtcNow.AddMinutes(20),
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
        var avatar = ctx.User.FindFirstValue("avatar");
        var role = ctx.User.FindFirstValue(ClaimTypes.Role);

        if (name is null || avatar is null || role is null)
        {
            return Results.Unauthorized();
        }

        return Results.Json(
            new
            {
                name,
                avatar,
                role
            }
        );
    }
}
