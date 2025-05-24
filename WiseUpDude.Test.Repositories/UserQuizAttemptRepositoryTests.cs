using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WiseUpDude.Data;
//using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories;
using WiseUpDude.Model;
using Xunit;

namespace WiseUpDude.Test.Repositories
{
    public class UserQuizAttemptRepositoryTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        private UserQuizAttemptRepository GetRepository(ApplicationDbContext dbContext)
        {
            var loggerMock = new Mock<ILogger<UserQuizAttemptRepository>>();
            return new UserQuizAttemptRepository(dbContext, loggerMock.Object);
        }

        [Fact]
        public async Task AddAsync_AddsUserQuizAttemptWithQuestions()
        {
            using var dbContext = GetInMemoryDbContext();
            var repository = GetRepository(dbContext);
            var userQuizAttempt = new UserQuizAttempt
            {
                UserQuizId = 1,
                AttemptDate = DateTime.UtcNow,
                Score = 80,
                Duration = TimeSpan.FromMinutes(10),
                AttemptQuestions = new List<UserQuizAttemptQuestion>
                {
                    new UserQuizAttemptQuestion { UserQuizQuestionId = 1, UserAnswer = "A", IsCorrect = true, TimeTakenSeconds = 30 },
                    new UserQuizAttemptQuestion { UserQuizQuestionId = 2, UserAnswer = "B", IsCorrect = false, TimeTakenSeconds = 45 }
                }
            };
            var addResult = await repository.AddAsync(userQuizAttempt);
            Assert.True(addResult.Id > 0);
            Assert.Equal(2, dbContext.UserQuizAttemptQuestions.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsUserQuizAttemptWithQuestions()
        {
            using var dbContext = GetInMemoryDbContext();
            var repository = GetRepository(dbContext);
            var userQuizAttempt = new UserQuizAttempt
            {
                UserQuizId = 2,
                AttemptDate = DateTime.UtcNow,
                Score = 90,
                Duration = TimeSpan.FromMinutes(5),
                AttemptQuestions = new List<UserQuizAttemptQuestion>
                {
                    new UserQuizAttemptQuestion { UserQuizQuestionId = 3, UserAnswer = "C", IsCorrect = true, TimeTakenSeconds = 20 }
                }
            };
            var addedAttempt = await repository.AddAsync(userQuizAttempt);
            var fetchedAttempt = await repository.GetByIdAsync(addedAttempt.Id);
            Assert.NotNull(fetchedAttempt);
            Assert.Single(fetchedAttempt.AttemptQuestions);
            Assert.Equal("C", fetchedAttempt.AttemptQuestions[0].UserAnswer);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesUserQuizAttemptAndQuestions()
        {
            using var dbContext = GetInMemoryDbContext();
            var repository = GetRepository(dbContext);
            var userQuizAttempt = new UserQuizAttempt
            {
                UserQuizId = 3,
                AttemptDate = DateTime.UtcNow,
                Score = 70,
                Duration = TimeSpan.FromMinutes(7),
                AttemptQuestions = new List<UserQuizAttemptQuestion>
                {
                    new UserQuizAttemptQuestion { UserQuizQuestionId = 4, UserAnswer = "D", IsCorrect = false, TimeTakenSeconds = 50 }
                }
            };
            var addedAttempt = await repository.AddAsync(userQuizAttempt);
            addedAttempt.Score = 100;
            addedAttempt.AttemptQuestions[0].UserAnswer = "E";
            await repository.UpdateAsync(addedAttempt);
            var updatedAttempt = await repository.GetByIdAsync(addedAttempt.Id);
            Assert.Equal(100, updatedAttempt.Score);
            Assert.Equal("E", updatedAttempt.AttemptQuestions[0].UserAnswer);
        }

        [Fact]
        public async Task DeleteAsync_RemovesUserQuizAttemptAndQuestions()
        {
            using var dbContext = GetInMemoryDbContext();
            var repository = GetRepository(dbContext);
            var userQuizAttempt = new UserQuizAttempt
            {
                UserQuizId = 4,
                AttemptDate = DateTime.UtcNow,
                Score = 60,
                Duration = TimeSpan.FromMinutes(8),
                AttemptQuestions = new List<UserQuizAttemptQuestion>
                {
                    new UserQuizAttemptQuestion { UserQuizQuestionId = 5, UserAnswer = "F", IsCorrect = true, TimeTakenSeconds = 40 }
                }
            };
            var addedAttempt = await repository.AddAsync(userQuizAttempt);
            await repository.DeleteAsync(addedAttempt.Id);
            var deletedAttempt = await repository.GetByIdAsync(addedAttempt.Id);
            Assert.Null(deletedAttempt);
            Assert.Empty(dbContext.UserQuizAttemptQuestions.Where(q => q.UserQuizAttemptId == addedAttempt.Id));
        }
    }
}
