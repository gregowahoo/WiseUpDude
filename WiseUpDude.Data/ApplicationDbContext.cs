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
    public DbSet<Category> Categories{ get; set; }

    public DbSet<UserQuiz> UserQuizzes { get; set; }
    public DbSet<UserQuizQuestion> UserQuizQuestions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure CreationDate for TopicCreationRun
        modelBuilder.Entity<TopicCreationRun>()
            .Property(t => t.CreationDate)
            .HasDefaultValueSql("GETUTCDATE()");

        // Configure CreationDate for Quiz
        modelBuilder.Entity<Quiz>()
            .Property(q => q.CreationDate)
            .HasDefaultValueSql("GETUTCDATE()");

        // Configure CreationDate for QuizQuestion
        modelBuilder.Entity<QuizQuestion>()
            .Property(qq => qq.CreationDate)
            .HasDefaultValueSql("GETUTCDATE()");

        // Configure CreationDate for Topic
        modelBuilder.Entity<Topic>()
            .Property(t => t.CreationDate)
            .HasDefaultValueSql("GETUTCDATE()");

        // Configure CreationDate for UserQuiz
        modelBuilder.Entity<UserQuiz>()
            .Property(uq => uq.CreationDate)
            .HasDefaultValueSql("GETUTCDATE()");

        // Configure CreationDate for UserQuizQuestion
        modelBuilder.Entity<UserQuizQuestion>()
            .Property(uqq => uqq.CreationDate)
            .HasDefaultValueSql("GETUTCDATE()");

        // Existing configuration for Quiz and Topic relationship
        modelBuilder.Entity<Quiz>()
            .HasOne(q => q.Topic)
            .WithMany(t => t.Quizzes)
            .HasForeignKey(q => q.TopicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
