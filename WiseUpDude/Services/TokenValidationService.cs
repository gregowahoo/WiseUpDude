using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace WiseUpDude.Services
{
    public interface ITokenValidationService
    {
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> IsTokenValidatedAsync();
        Task SetTokenValidatedAsync();
        Task ClearTokenValidationAsync();
    }

    public class TokenValidationService : ITokenValidationService
    {
        private readonly IConfiguration _configuration;
        private readonly ProtectedSessionStorage _sessionStorage;
        private const string SESSION_KEY = "AccessTokenValidated";

        public TokenValidationService(IConfiguration configuration, ProtectedSessionStorage sessionStorage)
        {
            _configuration = configuration;
            _sessionStorage = sessionStorage;
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            var configuredToken = _configuration["TestingAccess:SecretToken"];
            return Task.FromResult(!string.IsNullOrEmpty(configuredToken) && configuredToken == token);
        }

        public async Task<bool> IsTokenValidatedAsync()
        {
            try
            {
                var result = await _sessionStorage.GetAsync<bool>(SESSION_KEY);
                return result.Success && result.Value;
            }
            catch
            {
                return false;
            }
        }

        public async Task SetTokenValidatedAsync()
        {
            await _sessionStorage.SetAsync(SESSION_KEY, true);
        }

        public async Task ClearTokenValidationAsync()
        {
            await _sessionStorage.DeleteAsync(SESSION_KEY);
        }
    }
}