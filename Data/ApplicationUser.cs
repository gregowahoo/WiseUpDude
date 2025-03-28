using Microsoft.AspNetCore.Identity;
using WiseUpDude.Model;

namespace WiseUpDude.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}
