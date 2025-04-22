using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;

namespace WiseUpDude.Data.Repositories
{
    public class TopicCreationRunRepository
    {
        private readonly ApplicationDbContext _context;

        public TopicCreationRunRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Add a new TopicCreationRun with Model.Topic objects
        public async Task AddAsync(TopicCreationRun topicCreationRun, List<Model.Topic> modelTopics)
        {
            // Map Model.Topic to Entity.Topic
            var entityTopics = modelTopics.Select(mt => new WiseUpDude.Data.Entities.Topic
            {
                Name = mt.Name,
                Description = mt.Description,
                //CreationDate = mt.CreationDate,
                TopicCreationRunId = topicCreationRun.Id,
                TopicCreationRun = topicCreationRun // Set the required TopicCreationRun property
            }).ToList();

            // Assign the mapped topics to the TopicCreationRun
            topicCreationRun.Topics = entityTopics;

            // Add the TopicCreationRun to the database
            await _context.TopicCreationRuns.AddAsync(topicCreationRun);
            await _context.SaveChangesAsync();
        }

        // Get a TopicCreationRun by ID
        public async Task<TopicCreationRun?> GetByIdAsync(int id)
        {
            return await _context.TopicCreationRuns
                .Include(t => t.Topics) // Include related Topics
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // Get all TopicCreationRuns
        public async Task<List<TopicCreationRun>> GetAllAsync()
        {
            return await _context.TopicCreationRuns
                .Include(t => t.Topics) // Include related Topics
                .ToListAsync();
        }

        // Update an existing TopicCreationRun
        public async Task UpdateAsync(TopicCreationRun topicCreationRun)
        {
            _context.TopicCreationRuns.Update(topicCreationRun);
            await _context.SaveChangesAsync();
        }

        // Delete a TopicCreationRun by ID
        public async Task DeleteAsync(int id)
        {
            var topicCreationRun = await _context.TopicCreationRuns.FindAsync(id);
            if (topicCreationRun != null)
            {
                _context.TopicCreationRuns.Remove(topicCreationRun);
                await _context.SaveChangesAsync();
            }
        }
    }
}
