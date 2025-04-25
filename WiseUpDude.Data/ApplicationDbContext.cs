using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;

namespace WiseUpDude.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<TopicCreationRun> TopicCreationRuns { get; set; }

    public DbSet<UserQuiz> UserQuizzes { get; set; }
    public DbSet<UserQuizQuestion> UserQuizQuestions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TopicCreationRun>()
            .Property(t => t.CreationDate)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<Quiz>()
            .HasOne(q => q.Topic)
            .WithMany(t => t.Quizzes) // Add a collection property in `Topic` if needed
            .HasForeignKey(q => q.TopicId)
            .OnDelete(DeleteBehavior.Cascade);

        // Add any specific configurations for UserQuiz and UserQuizQuestion if needed
    }
}
