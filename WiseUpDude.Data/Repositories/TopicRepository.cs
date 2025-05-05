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
        private readonly ApplicationDbContext _context;

        public TopicRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public string? Category { get; set; }
        public string? CategoryDescription { get; set; }

        public async Task<IEnumerable<WiseUpDude.Model.Topic>> GetAllAsync()
        {
            var entities = await _context.Topics
                .Include(t => t.Category) // Include the related Category
                .ToListAsync();

            return entities.Select(e => new WiseUpDude.Model.Topic
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                CategoryId = e.CategoryId ?? 0, // Handle nullable CategoryId
                Category = e.Category?.Name ?? "Uncategorized", // Handle null Category
                CategoryDescription = e.Category?.Description ?? string.Empty // Handle null Category
            });
        }

        public async Task<IEnumerable<WiseUpDude.Model.Topic>> GetUniqueTopicsAsync()
        {
            var uniqueTopics = await _context.Topics
                .Include(t => t.Category) // Include the related Category
                .GroupBy(t => t.Name) // Group by the Name property to ensure uniqueness
                .Select(g => g.First()) // Select the first topic in each group
                .ToListAsync();

            return uniqueTopics.Select(topic => new WiseUpDude.Model.Topic
            {
                Id = topic.Id,
                Name = topic.Name,
                Description = topic.Description,
                CategoryId = topic.CategoryId ?? 0, // Handle nullable CategoryId
                Category = topic.Category?.Name ?? "Uncategorized", // Handle null Category
                CategoryDescription = topic.Category?.Description ?? string.Empty // Handle null Category
            });
        }

        public async Task<WiseUpDude.Model.Topic> GetByIdAsync(int id)
        {
            var entity = await _context.Topics
                .Include(t => t.Category) // Include the related Category
                .FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null)
                throw new KeyNotFoundException($"Topic with Id {id} not found.");

            return new WiseUpDude.Model.Topic
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                CategoryId = entity.CategoryId ?? 0, // Handle nullable CategoryId
                Category = entity.Category?.Name ?? "Uncategorized", // Handle null Category
                CategoryDescription = entity.Category?.Description ?? string.Empty // Handle null Category
            };
        }

        public async Task<IEnumerable<WiseUpDude.Model.Topic>> GetTopicsAsync(int count)
        {
            var topics = await _context.Topics
                .Include(t => t.Category) // Include the related Category
                .Take(count) // Limit the number of topics returned
                .ToListAsync();

            return topics.Select(topic => new WiseUpDude.Model.Topic
            {
                Id = topic.Id,
                Name = topic.Name,
                Description = topic.Description,
                CategoryId = topic.CategoryId ?? 0, // Handle nullable CategoryId
                Category = topic.Category?.Name ?? "Uncategorized", // Handle null Category
                CategoryDescription = topic.Category?.Description ?? string.Empty // Handle null Category
            });
        }

        public async Task<IEnumerable<WiseUpDude.Model.Topic>> GetTopicsWithoutQuestionsAsync()
        {
            var topicsWithoutQuestions = await _context.Topics
                .Include(t => t.Category) // Include the related Category
                .Where(topic => !topic.Quizzes.Any()) // Check if the topic has no associated quizzes
                .ToListAsync();

            return topicsWithoutQuestions.Select(topic => new WiseUpDude.Model.Topic
            {
                Id = topic.Id,
                Name = topic.Name,
                Description = topic.Description,
                CategoryId = topic.CategoryId ?? 0, // Handle nullable CategoryId
                Category = topic.Category?.Name ?? "Uncategorized", // Handle null Category
                CategoryDescription = topic.Category?.Description ?? string.Empty // Handle null Category
            });
        }

        public async Task<IEnumerable<WiseUpDude.Model.Topic>> GetTopicsByCategoryAsync(int categoryId)
        {
            var topics = await _context.Topics
                .Include(t => t.Category) // Include the related Category
                .Where(t => t.CategoryId == categoryId) // Filter by CategoryId
                .ToListAsync();

            return topics.Select(topic => new WiseUpDude.Model.Topic
            {
                Id = topic.Id,
                Name = topic.Name,
                Description = topic.Description,
                CategoryId = topic.CategoryId ?? 0, // Handle nullable CategoryId
                Category = topic.Category?.Name ?? "Uncategorized", // Handle null Category
                CategoryDescription = topic.Category?.Description ?? string.Empty // Handle null Category.Description
            });
        }

        public async Task AddAsync(WiseUpDude.Model.Topic topic)
        {
            var categoryEntity = await _context.Categories.FindAsync(topic.CategoryId);
            if (categoryEntity == null)
                throw new KeyNotFoundException($"Category with Id {topic.CategoryId} not found.");

            var entity = new Entities.Topic
            {
                Name = topic.Name,
                Description = topic.Description,
                CategoryId = topic.CategoryId, // No change needed here
                Category = categoryEntity, // Set required Category navigation property
                TopicCreationRun = new Entities.TopicCreationRun
                {
                    Llm = "DefaultLlmValue"
                }
            };

            _context.Topics.Add(entity);
            await _context.SaveChangesAsync();

            topic.Id = entity.Id;
        }

        public async Task UpdateAsync(WiseUpDude.Model.Topic topic)
        {
            var entity = await _context.Topics.FirstOrDefaultAsync(t => t.Id == topic.Id);

            if (entity == null)
                throw new KeyNotFoundException($"Topic with Id {topic.Id} not found.");

            entity.Name = topic.Name;
            entity.Description = topic.Description;
            entity.CategoryId = topic.CategoryId; // No change needed here

            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Topics.FirstOrDefaultAsync(t => t.Id == id);

            if (entity != null)
            {
                _context.Topics.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}

