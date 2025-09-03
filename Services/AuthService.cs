using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace cleancityApp.Services;

public class AuthService
{
    private readonly IJSRuntime _js;
    private readonly FakeAuthStateProvider _provider;
    public const string Key = "demo_auth";

    public AuthService(IJSRuntime js, FakeAuthStateProvider provider)
    {
        _js = js; _provider = provider;
    }

    public async Task<bool> IsLoggedInAsync()
        => (await _js.InvokeAsync<string>("appStorage.get", Key)) == "1";

    public async Task LoginAsync(string user, string pass)
    {
        if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
        {
            await _js.InvokeVoidAsync("appStorage.set", Key, "1");
            _provider.SetAuthenticated(user);
        }
    }

    public async Task LogoutAsync()
    {
        await _js.InvokeVoidAsync("appStorage.remove", Key);
        _provider.SetLoggedOut();
    }
}

public class FakeAuthStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _principal = new(new ClaimsIdentity());

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(new AuthenticationState(_principal));

    public void SetAuthenticated(string username)
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }, "FakeAuth");
        _principal = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void SetLoggedOut()
    {
        _principal = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
