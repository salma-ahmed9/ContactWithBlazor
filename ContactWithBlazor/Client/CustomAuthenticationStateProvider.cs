using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using System.Net.Http.Json;
using System.Security.Claims;

namespace ContactWithBlazor.Client
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;

        public CustomAuthenticationStateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            LoginInput currentUser = await _httpClient.GetFromJsonAsync<LoginInput>("/getuser");
            if (currentUser != null && currentUser.UserName != null)
            {
                var claims = new List<Claim>
                {
                   new Claim(ClaimTypes.Name, currentUser.UserName),
                   new Claim(ClaimTypes.Role, currentUser.Role),
                };
                var scheme = CookieAuthenticationDefaults.AuthenticationScheme;
                var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = true

                };
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                return new AuthenticationState(claimsPrincipal);
            }
            else
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }


        }
    }
}
