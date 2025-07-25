using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace WiseUpDude.Services
{
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
            return Task.FromResult(!string.IsNullOrEmpty(configuredToken) &&
                string.Equals(configuredToken, token, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> IsTokenValidatedAsync()
        {
            try
            {
                var result = await _sessionStorage.GetAsync<bool>(SESSION_KEY);
                return result.Success && result.Value;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop calls cannot be issued at this time"))
            {
                // During prerendering, JavaScript interop is not available
                // Return false to indicate token is not validated yet
                return false;
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
