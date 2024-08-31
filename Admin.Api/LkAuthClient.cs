using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using PResult;

namespace Admin.Api;

public sealed class AuthResponse
{
    public required string PersonGuid { get; init; }
    public required string FullName { get; init; }
    public required string PictureUrl { get; init; }
}

public sealed class WrongUsernameOrPasswordError : Exception
{
    public WrongUsernameOrPasswordError()
        : base("Wrong username or password") { }
}

public sealed class LkAuthClient
{
    private readonly HttpClient _httpClient;

    public LkAuthClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<AuthResponse>> Authenticate(string username, string password)
    {
        var req = new Dictionary<string, string>
        {
            { "ulogin", username },
            { "upassword", password },
        };

        var authResponse = await _httpClient.PostAsync(
            "old/lk_api.php",
            new FormUrlEncodedContent(req)
        );

        if (authResponse.StatusCode != HttpStatusCode.OK)
        {
            return new WrongUsernameOrPasswordError();
        }

        var authInfo = await JsonSerializer.DeserializeAsync<LKAuthResponse>(
            await authResponse.Content.ReadAsStreamAsync()
        );

        var userResponse = await _httpClient.GetAsync(
            $"old/lk_api.php/?getAppData&token={authInfo.Token}"
        );

        var avatarResponse = await _httpClient.GetAsync(
            $"old/lk_api.php/?getUser&token={authInfo.Token}"
        );

        if (
            userResponse.StatusCode != HttpStatusCode.OK
            || avatarResponse.StatusCode != HttpStatusCode.OK
        )
        {
            throw new Exception("Unknown HTTP error");
        }

        var userInfo = await JsonSerializer.DeserializeAsync<LKUserResponse>(
            await userResponse.Content.ReadAsStreamAsync()
        );

        var avatarInfo = await JsonSerializer.DeserializeAsync<LKUserAvatarResponse>(
            await avatarResponse.Content.ReadAsStreamAsync()
        );

        return new AuthResponse
        {
            FullName = $"{userInfo.Surname} {userInfo.Name} {userInfo.Patronymic}".Trim(),
            PersonGuid = userInfo.PersonGuid,
            PictureUrl = avatarInfo.User.AvatarUrl,
        };
    }
}

file struct LKAuthResponse
{
    [JsonPropertyName("token")]
    public required string Token { get; init; }
}

file struct LKUserResponse
{
    [JsonPropertyName("guid_person")]
    public required string PersonGuid { get; init; }

    [JsonPropertyName("surname")]
    public required string Surname { get; init; }

    [JsonPropertyName("patronymic")]
    public required string Patronymic { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }
}

file struct LKUserAvatarResponse
{
    [JsonPropertyName("user")]
    public required User User { get; init; }
}

file struct User
{
    [JsonPropertyName("avatar")]
    public required string AvatarUrl { get; init; }
}
