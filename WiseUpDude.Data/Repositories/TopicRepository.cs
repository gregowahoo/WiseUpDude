using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class TopicRepository : IUserRepository<Model.Topic>
    {
        private readonly ApplicationDbContext _context;

        public TopicRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Model.Topic>> GetAllAsync()
        {
            var entities = await _context.Topics.ToListAsync();

            return entities.Select(e => new Model.Topic
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description
            });
        }

        public async Task<Model.Topic> GetByIdAsync(int id)
        {
            var entity = await _context.Topics.FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null)
                throw new KeyNotFoundException($"Topic with Id {id} not found.");

            return new Model.Topic
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description
            };
        }

        public async Task AddAsync(Model.Topic topic)
        {
            var entity = new Entities.Topic
            {
                Name = topic.Name,
                Description = topic.Description,
                TopicCreationRun = new TopicCreationRun
                {
                    Llm = "DefaultLlmValue" // Provide a default or meaningful value for the required property
                }
            };

            _context.Topics.Add(entity);
            await _context.SaveChangesAsync();

            topic.Id = entity.Id;
        }

        public async Task AddAsync(Model.Topic topic, int topicCreationRunId)
        {
            var entity = new Entities.Topic
            {
                Name = topic.Name,
                Description = topic.Description,
                TopicCreationRunId = topicCreationRunId,
                TopicCreationRun = new TopicCreationRun // Initialize the required TopicCreationRun property
                {
                    Id = topicCreationRunId
                }
            };

            _context.Topics.Add(entity);
            await _context.SaveChangesAsync();

            topic.Id = entity.Id;
        }

        public async Task UpdateAsync(Model.Topic topic)
        {
            var entity = await _context.Topics.FirstOrDefaultAsync(t => t.Id == topic.Id);

            if (entity == null)
                throw new KeyNotFoundException($"Topic with Id {topic.Id} not found.");

            entity.Name = topic.Name;
            entity.Description = topic.Description;

            if (entity.TopicCreationRun == null)
            {
                entity.TopicCreationRun = new TopicCreationRun
                {
                    Llm = "DefaultLlmValue" // Provide a default or meaningful value for the required property
                };
            }

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

        public async Task<IEnumerable<Model.Topic>> GetTopicsWithoutQuestionsAsync()
        {
            var topicsWithoutQuestions = await _context.Topics
                .Where(topic => !_context.QuizQuestions.Any(q => q.Quiz.Topic == topic.Name))
                .ToListAsync();

            return topicsWithoutQuestions.Select(topic => new Model.Topic
            {
                Id = topic.Id,
                Name = topic.Name,
                Description = topic.Description
            });
        }
    }
}
