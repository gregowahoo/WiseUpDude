using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class TopicRepository : ITopicRepository<WiseUpDude.Model.Topic>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public TopicRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }
        public string? Category { get; set; }
        public string? CategoryDescription { get; set; }

        public async Task<IEnumerable<WiseUpDude.Model.Topic>> GetAllAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entities = await context.Topics
                .Include(t => t.Category)
                .ToListAsync();
            return entities.Select(e => new WiseUpDude.Model.Topic
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                CategoryId = e.CategoryId ?? 0,
                Category = e.Category?.Name ?? "Uncategorized",
                CategoryDescription = e.Category?.Description ?? string.Empty
            });
        }

        public async Task<IEnumerable<WiseUpDude.Model.Topic>> GetUniqueTopicsAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var uniqueTopics = await context.Topics
                .Include(t => t.Category)
                .GroupBy(t => t.Name)
                .Select(g => g.First())
                .ToListAsync();
            return uniqueTopics.Select(topic => new WiseUpDude.Model.Topic
            {
                Id = topic.Id,
                Name = topic.Name,
                Description = topic.Description,
                CategoryId = topic.CategoryId ?? 0,
                Category = topic.Category?.Name ?? "Uncategorized",
                CategoryDescription = topic.Category?.Description ?? string.Empty
            });
        }

        public async Task<WiseUpDude.Model.Topic> GetByIdAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.Topics
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null)
                throw new KeyNotFoundException($"Topic with Id {id} not found.");
            return new WiseUpDude.Model.Topic
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                CategoryId = entity.CategoryId ?? 0,
                Category = entity.Category?.Name ?? "Uncategorized",
                CategoryDescription = entity.Category?.Description ?? string.Empty
            };
        }

        public async Task<IEnumerable<WiseUpDude.Model.Topic>> GetTopicsAsync(int count)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var topics = await context.Topics
                .Include(t => t.Category)
                .Take(count)
                .ToListAsync();
            return topics.Select(topic => new WiseUpDude.Model.Topic
            {
                Id = topic.Id,
                Name = topic.Name,
                Description = topic.Description,
                CategoryId = topic.CategoryId ?? 0,
                Category = topic.Category?.Name ?? "Uncategorized",
                CategoryDescription = topic.Category?.Description ?? string.Empty
            });
        }

        public async Task<IEnumerable<WiseUpDude.Model.Topic>> GetTopicsWithoutQuestionsAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var topicsWithoutQuestions = await context.Topics
                .Include(t => t.Category)
                .Where(topic => !topic.Quizzes.Any())
                .ToListAsync();
            return topicsWithoutQuestions.Select(topic => new WiseUpDude.Model.Topic
            {
                Id = topic.Id,
                Name = topic.Name,
                Description = topic.Description,
                CategoryId = topic.CategoryId ?? 0,
                Category = topic.Category?.Name ?? "Uncategorized",
                CategoryDescription = topic.Category?.Description ?? string.Empty
            });
        }

        public async Task<IEnumerable<WiseUpDude.Model.Topic>> GetTopicsByCategoryAsync(int categoryId)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var topics = await context.Topics
                .Include(t => t.Category)
                .Where(t => t.CategoryId == categoryId)
                .ToListAsync();
            return topics.Select(topic => new WiseUpDude.Model.Topic
            {
                Id = topic.Id,
                Name = topic.Name,
                Description = topic.Description,
                CategoryId = topic.CategoryId ?? 0,
                Category = topic.Category?.Name ?? "Uncategorized",
                CategoryDescription = topic.Category?.Description ?? string.Empty
            });
        }

        public async Task AddAsync(WiseUpDude.Model.Topic topic)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var categoryEntity = await context.Categories.FindAsync(topic.CategoryId);
            if (categoryEntity == null)
                throw new KeyNotFoundException($"Category with Id {topic.CategoryId} not found.");
            var entity = new Entities.Topic
            {
                Name = topic.Name,
                Description = topic.Description,
                CategoryId = topic.CategoryId,
                Category = categoryEntity,
                TopicCreationRun = new Entities.TopicCreationRun
                {
                    Llm = "DefaultLlmValue"
                }
            };
            context.Topics.Add(entity);
            await context.SaveChangesAsync();
            topic.Id = entity.Id;
        }

        public async Task UpdateAsync(WiseUpDude.Model.Topic topic)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.Topics.FirstOrDefaultAsync(t => t.Id == topic.Id);
            if (entity == null)
                throw new KeyNotFoundException($"Topic with Id {topic.Id} not found.");
            entity.Name = topic.Name;
            entity.Description = topic.Description;
            entity.CategoryId = topic.CategoryId;
            context.Entry(entity).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.Topics.FirstOrDefaultAsync(t => t.Id == id);
            if (entity != null)
            {
                context.Topics.Remove(entity);
                await context.SaveChangesAsync();
            }
        }
    }
}

