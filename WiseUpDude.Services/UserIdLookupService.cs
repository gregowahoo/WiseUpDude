using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WiseUpDude.Data;

namespace WiseUpDude.Services
{
    public interface IUserIdLookupService
    {
        Task<string?> GetUserIdByEmailAsync(string email);
    }

    public class UserIdLookupService : IUserIdLookupService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserIdLookupService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string?> GetUserIdByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user?.Id;
        }
    }
}