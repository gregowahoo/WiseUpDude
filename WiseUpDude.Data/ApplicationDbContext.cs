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
    public DbSet<Category> Categories { get; set; }

    public DbSet<UserQuiz> UserQuizzes { get; set; }
    public DbSet<UserQuizQuestion> UserQuizQuestions { get; set; }

    public DbSet<UserQuizAttempt> UserQuizAttempt { get; set; }
    public DbSet<UserQuizAttemptQuestion> UserQuizAttemptQuestion { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure CreationDate for TopicCreationRun
        modelBuilder.Entity<TopicCreationRun>()
            .Property(t => t.CreationDate)
            .HasDefaultValueSql("GETDATE()");

        // Configure CreationDate for Quiz
        modelBuilder.Entity<Quiz>()
            .Property(q => q.CreationDate)
            .HasDefaultValueSql("GETDATE()");

        // Configure CreationDate for QuizQuestion
        modelBuilder.Entity<QuizQuestion>()
            .Property(qq => qq.CreationDate)
            .HasDefaultValueSql("GETDATE()");

        // Configure CreationDate for Topic
        modelBuilder.Entity<Topic>()
            .Property(t => t.CreationDate)
            .HasDefaultValueSql("GETDATE()");

        // Configure CreationDate for UserQuiz
        modelBuilder.Entity<UserQuiz>()
            .Property(uq => uq.CreationDate)
            .HasDefaultValueSql("GETDATE()");

        // Configure CreationDate for UserQuizQuestion
        modelBuilder.Entity<UserQuizQuestion>()
            .Property(uqq => uqq.CreationDate)
            .HasDefaultValueSql("GETDATE()");

        // Configure CreationDate for UserQuizQuestion
        modelBuilder.Entity<Category>()
            .Property(uqq => uqq.CreationDate)
            .HasDefaultValueSql("GETDATE()");

        // Existing configuration for Quiz and Topic relationship
        modelBuilder.Entity<Quiz>()
            .HasOne(q => q.Topic)
            .WithMany(t => t.Quizzes)
            .HasForeignKey(q => q.TopicId)
            .OnDelete(DeleteBehavior.Cascade);

        // Fix cascade delete issue
        modelBuilder.Entity<UserQuizAttemptQuestion>()
            .HasOne(uqaq => uqaq.UserQuizAttempt)
            .WithMany(uqa => uqa.AttemptQuestions)
            .HasForeignKey(uqaq => uqaq.UserQuizAttemptId)
            .OnDelete(DeleteBehavior.Restrict);  // <-- set to Restrict instead of Cascade

        modelBuilder.Entity<UserQuizAttemptQuestion>()
            .HasOne(aq => aq.UserQuizQuestion)
            .WithMany()
            .HasForeignKey(aq => aq.UserQuizQuestionId)
            .OnDelete(DeleteBehavior.Restrict); // Protect frozen data
    }
}
