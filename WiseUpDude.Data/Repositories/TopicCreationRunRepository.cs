using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class TopicCreationRunRepository : ITopicCreationRunRepository<Model.TopicCreationRun>
    {
        private readonly ApplicationDbContext _context;

        public TopicCreationRunRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Model.TopicCreationRun entity)
        {
            // Map Model.TopicCreationRun to Entity.TopicCreationRun
            var entityTopicCreationRun = new WiseUpDude.Data.Entities.TopicCreationRun
            {
                CreationDate = entity.CreationDate,
                Llm = entity.Llm,
                Topics = entity.Topics?.Select(t => new WiseUpDude.Data.Entities.Topic
                {
                    Name = t.Name,
                    Description = t.Description,
                    TopicCreationRunId = entity.Id, // Set the foreign key
                    TopicCreationRun = new WiseUpDude.Data.Entities.TopicCreationRun
                    {
                        Id = entity.Id,
                        CreationDate = entity.CreationDate,
                        Llm = entity.Llm
                    } // Map the required TopicCreationRun property
                }).ToList() ?? new List<WiseUpDude.Data.Entities.Topic>() // Handle possible null reference
            };

            // Add the TopicCreationRun to the database
            await _context.TopicCreationRuns.AddAsync(entityTopicCreationRun);
            await _context.SaveChangesAsync();

            // Update the model ID with the generated entity ID
            entity.Id = entityTopicCreationRun.Id;
        }

        public async Task AddAsync(Model.TopicCreationRun topicCreationRun, List<Model.Topic> modelTopics)
        {
            // Map Model.Topic to Entity.Topic
            var entityTopics = modelTopics.Select(mt => new WiseUpDude.Data.Entities.Topic
            {
                Name = mt.Name,
                Description = mt.Description,
                TopicCreationRunId = topicCreationRun.Id,
                TopicCreationRun = new WiseUpDude.Data.Entities.TopicCreationRun
                {
                    Id = topicCreationRun.Id,
                    CreationDate = topicCreationRun.CreationDate,
                    Llm = topicCreationRun.Llm
                } // Map Model.TopicCreationRun to Entity.TopicCreationRun
            }).ToList();

            // Create a new Entity.TopicCreationRun and assign the mapped topics
            var entityTopicCreationRun = new WiseUpDude.Data.Entities.TopicCreationRun
            {
                Id = topicCreationRun.Id,
                CreationDate = topicCreationRun.CreationDate,
                Llm = topicCreationRun.Llm,
                Topics = entityTopics
            };

            // Add the TopicCreationRun to the database
            await _context.TopicCreationRuns.AddAsync(entityTopicCreationRun);
            await _context.SaveChangesAsync();

            // Update the model ID with the generated entity ID
            topicCreationRun.Id = entityTopicCreationRun.Id;
        }

        // Get a TopicCreationRun by ID
        public async Task<Model.TopicCreationRun> GetByIdAsync(int id)
        {
            var entity = await _context.TopicCreationRuns
                .Include(t => t.Topics) // Include related Topics
                .FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null)
                throw new KeyNotFoundException($"TopicCreationRun with Id {id} not found.");

            // Map Entity.TopicCreationRun to Model.TopicCreationRun
            return new Model.TopicCreationRun
            {
                Id = entity.Id,
                CreationDate = entity.CreationDate,
                Llm = entity.Llm,
                Topics = entity.Topics?.Select(t => new Model.Topic
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description
                }).ToList()
            };
        }

        // Get all TopicCreationRuns
        public async Task<IEnumerable<Model.TopicCreationRun>> GetAllAsync()
        {
            var entities = await _context.TopicCreationRuns
                .Include(t => t.Topics) // Include related Topics
                .ToListAsync();

            // Map Entity.TopicCreationRun to Model.TopicCreationRun
            return entities.Select(entity => new Model.TopicCreationRun
            {
                Id = entity.Id,
                CreationDate = entity.CreationDate,
                Llm = entity.Llm,
                Topics = entity.Topics?.Select(t => new Model.Topic
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description
                }).ToList()
            });
        }

        // Update an existing TopicCreationRun
        public async Task UpdateAsync(Model.TopicCreationRun model)
        {
            var entity = await _context.TopicCreationRuns
                .Include(t => t.Topics)
                .FirstOrDefaultAsync(t => t.Id == model.Id);

            if (entity == null)
                throw new KeyNotFoundException($"TopicCreationRun with Id {model.Id} not found.");

            // Update entity properties
            entity.Llm = model.Llm;
            entity.CreationDate = model.CreationDate;

            // Update topics (optional, depending on your requirements)
            if (model.Topics != null)
            {
                entity.Topics = model.Topics.Select(t => new Entities.Topic
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    TopicCreationRunId = entity.Id,
                    TopicCreationRun = entity // Set the required TopicCreationRun property
                }).ToList();
            }

            _context.TopicCreationRuns.Update(entity);
            await _context.SaveChangesAsync();
        }

        // Delete a TopicCreationRun by ID
        public async Task DeleteAsync(int id)
        {
            var entity = await _context.TopicCreationRuns.FindAsync(id);
            if (entity != null)
            {
                _context.TopicCreationRuns.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
