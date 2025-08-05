using Blazored.LocalStorage;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WiseUpDude.Client.Services
{
    public class JwtAuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;
        private const string TokenKey = "jwt_token";
        public JwtAuthService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }
        public async Task<bool> LoginAsync(string username, string password)
        {
            var resp = await _http.PostAsJsonAsync("/api/account/login", new { Username = username, Password = password });
            if (!resp.IsSuccessStatusCode) return false;
            var json = await resp.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("token").GetString();
            if (string.IsNullOrEmpty(token)) return false;
            await _localStorage.SetItemAsync(TokenKey, token);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return true;
        }
        public async Task<string?> GetTokenAsync()
        {
            return await _localStorage.GetItemAsync<string>(TokenKey);
        }
        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync(TokenKey);
            _http.DefaultRequestHeaders.Authorization = null;
        }
        public async Task<bool> IsAdminAsync()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        }
    }
}
