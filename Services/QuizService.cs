using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using WiseUpDude.Data;
using WiseUpDude.Model;

public class QuizService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public QuizService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task CreateQuizAsync(Quiz quiz, string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user != null)
        {
            quiz.UserId = user.Id;
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();
        }
    }
}
