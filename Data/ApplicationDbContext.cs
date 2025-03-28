using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Model;

namespace WiseUpDude.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<QuizModel> Quizzes { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<QuizModel>()
            .HasOne(q => q.User)
            .WithMany(u => u.Quizzes)
            .HasForeignKey(q => q.UserId);

        modelBuilder.Entity<QuizModel>()
            .Property(q => q.RowVersion)
            .IsRowVersion();

        modelBuilder.Entity<QuizQuestion>()
            .HasOne(qq => qq.Quiz)
            .WithMany(q => q.Questions)
            .HasForeignKey(qq => qq.QuizId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
