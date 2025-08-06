using Humanizer;
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

    public DbSet<UserQuizAttempt> UserQuizAttempts { get; set; }
    public DbSet<UserQuizAttemptQuestion> UserQuizAttemptQuestions { get; set; }

    public DbSet<LearningTrack> LearningTracks { get; set; }
    public DbSet<LearningTrackCategory> LearningTrackCategories { get; set; }
    public DbSet<LearningTrackSource> LearningTrackSources { get; set; }
    public DbSet<LearningTrackQuiz> LearningTrackQuizzes { get; set; }
    public DbSet<LearningTrackQuizQuestion> LearningTrackQuizQuestions { get; set; }
    public DbSet<LearningTrackQuizAttempt> LearningTrackQuizAttempts { get; set; }
    public DbSet<LearningTrackQuizAttemptQuestion> LearningTrackAttemptQuestions { get; set; }

    public DbSet<AssignmentType> AssignmentTypes { get; set; }
    public DbSet<SpecialQuizAssignment> SpecialQuizAssignments { get; set; }

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

        modelBuilder.Entity<UserQuiz>(entity =>
        {
            entity.HasKey(uq => uq.Id); // Set Id as the primary key
            entity.Property(uq => uq.Id)
                  .ValueGeneratedOnAdd(); // Ensure Id is auto-incrementing
            entity.Property(uq => uq.CreationDate)
                  .HasDefaultValueSql("GETDATE()"); // Configure default value for CreationDate
        });

        // Configure CreationDate for UserQuizQuestion
        modelBuilder.Entity<UserQuizQuestion>()
            .Property(uqq => uqq.CreationDate)
            .HasDefaultValueSql("GETDATE()");

        // Configure CreationDate for UserQuizQuestion
        modelBuilder.Entity<Category>()
            .Property(uqq => uqq.CreationDate)
            .HasDefaultValueSql("GETDATE()");

        // Configure AssignmentType entity
        modelBuilder.Entity<AssignmentType>(entity =>
        {
            entity.HasKey(at => at.Id);
            entity.Property(at => at.Id).ValueGeneratedOnAdd();
            entity.HasData(
                new AssignmentType { Id = 1, Name = "Featured", Description = "Quizzes selected for special prominence." },
                new AssignmentType { Id = 2, Name = "Seniors Need To Know", Description = "Practical, health, safety, and life skills quizzes for older adults." },
                new AssignmentType { Id = 3, Name = "Fun Facts", Description = "Quizzes packed with surprising, quirky, or little-known facts." },
                new AssignmentType { Id = 4, Name = "Wow", Description = "Quizzes with mind-blowing, record-breaking, or 'did you know?' content." },
                new AssignmentType { Id = 5, Name = "Brain Boosters", Description = "Puzzles, logic, and memory challenges to keep minds sharp." },
                new AssignmentType { Id = 6, Name = "History Mysteries", Description = "Quizzes about unsolved historical events or famous mysteries." },
                new AssignmentType { Id = 7, Name = "Tech Trends", Description = "Quizzes about the latest in technology, gadgets, and digital life." },
                new AssignmentType { Id = 8, Name = "Pop Culture Picks", Description = "Quizzes on movies, music, celebrities, and viral trends." },
                new AssignmentType { Id = 9, Name = "Travel Treasures", Description = "Quizzes about world geography, cultures, and travel tips." },
                new AssignmentType { Id = 10, Name = "Health Wellness", Description = "Nutrition, exercise, and mental health quizzes." },
                new AssignmentType { Id = 11, Name = "Financial Smarts", Description = "Quizzes on saving, investing, and money management." },
                new AssignmentType { Id = 12, Name = "Science Wonders", Description = "Quizzes about amazing discoveries and scientific phenomena." },
                new AssignmentType { Id = 13, Name = "Literary Legends", Description = "Quizzes on classic books, authors, and literary trivia." },
                new AssignmentType { Id = 14, Name = "Everyday Hacks", Description = "Quizzes with tips and tricks for daily life." },
                new AssignmentType { Id = 15, Name = "Local Lore", Description = "Quizzes about local history, landmarks, and traditions." },
                new AssignmentType { Id = 16, Name = "Green Living", Description = "Quizzes on sustainability, nature, and eco-friendly habits." }
            );
        });

        // Configure SpecialQuizAssignment entity
        modelBuilder.Entity<SpecialQuizAssignment>(entity =>
        {
            entity.HasKey(sqa => sqa.Id);
            entity.Property(sqa => sqa.Id).ValueGeneratedOnAdd();
            entity.Property(sqa => sqa.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.HasOne<UserQuiz>()
                .WithMany()
                .HasForeignKey(sqa => sqa.UserQuizId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<AssignmentType>()
                .WithMany()
                .HasForeignKey(sqa => sqa.AssignmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Existing configuration for Quiz and Topic relationship
        modelBuilder.Entity<Quiz>()
            .HasOne(q => q.Topic)
            .WithMany(t => t.Quizzes)
            .HasForeignKey(q => q.TopicId)
            .OnDelete(DeleteBehavior.Cascade);

        // Fix cascade delete issue - maintain relationship with UserQuizAttempt
        modelBuilder.Entity<UserQuizAttemptQuestion>()
            .HasOne(uqaq => uqaq.UserQuizAttempt)
            .WithMany(uqa => uqa.AttemptQuestions)
            .HasForeignKey(uqaq => uqaq.UserQuizAttemptId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Removed the relationship configuration with UserQuizQuestion
        // Only keeping the foreign key without navigation property

        modelBuilder.Entity<UserQuizAttempt>().ToTable("UserQuizAttempts");
        modelBuilder.Entity<UserQuizAttemptQuestion>().ToTable("UserQuizAttemptQuestions");

        // LearningTrack relationships
        modelBuilder.Entity<LearningTrack>()
            .Property(lt => lt.CreationDate)
            .HasDefaultValueSql("GETDATE()");
        modelBuilder.Entity<LearningTrack>()
            .HasMany(lt => lt.Categories)
            .WithOne(c => c.LearningTrack)
            .HasForeignKey(c => c.LearningTrackId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<LearningTrackCategory>()
            .HasMany(c => c.Sources)
            .WithOne(s => s.LearningTrackCategory)
            .HasForeignKey(s => s.LearningTrackCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<LearningTrackSource>()
            .HasMany(s => s.Quizzes)
            .WithOne(q => q.LearningTrackSource)
            .HasForeignKey(q => q.LearningTrackSourceId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<LearningTrackQuiz>()
            .HasMany(q => q.Questions)
            .WithOne(qq => qq.LearningTrackQuiz)
            .HasForeignKey(qq => qq.LearningTrackQuizId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LearningTrackCategory>()
            .Property(c => c.CreationDate)
            .HasDefaultValueSql("GETDATE()");
        modelBuilder.Entity<LearningTrackSource>()
            .Property(s => s.CreationDate)
            .HasDefaultValueSql("GETDATE()");
        modelBuilder.Entity<LearningTrackQuiz>()
            .Property(q => q.CreationDate)
            .HasDefaultValueSql("GETDATE()");
        modelBuilder.Entity<LearningTrackQuizQuestion>()
            .Property(qq => qq.CreationDate)
            .HasDefaultValueSql("GETDATE()");

        // LearningTrackQuizAttempt relationships
        modelBuilder.Entity<LearningTrackQuizAttempt>()
            .HasMany(lta => lta.AttemptQuestions)
            .WithOne(ltq => ltq.LearningTrackAttempt)
            .HasForeignKey(ltq => ltq.LearningTrackAttemptId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<LearningTrackQuizAttempt>().ToTable("LearningTrackQuizAttempts");
        modelBuilder.Entity<LearningTrackQuizAttemptQuestion>().ToTable("LearningTrackAttemptQuestions");

        //foreach (var entity in modelBuilder.Model.GetEntityTypes())
        //{
        //    // Use Humanizer to pluralize table names
        //    entity.SetTableName(entity.GetTableName().Pluralize());
        //}
    }
}
