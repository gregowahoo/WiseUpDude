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
                    CategoryId = t.CategoryId, // use CategoryId from the model
                    TopicCreationRunId = entity.Id,
                    TopicCreationRun = new WiseUpDude.Data.Entities.TopicCreationRun
                    {
                        Id = entity.Id,
                        CreationDate = entity.CreationDate,
                        Llm = entity.Llm
                    },
                    Category = new WiseUpDude.Data.Entities.Category // Ensure required member 'Category' is set
                    {
                        //Id = t.CategoryId,
                        Name = t.Category,
                        Description = t.CategoryDescription
                    }
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
            // Ensure categories are added to the database and get their IDs
            var categoryMap = new Dictionary<string, WiseUpDude.Data.Entities.Category>();
            foreach (var topic in modelTopics)
            {
                if (!string.IsNullOrWhiteSpace(topic.Category))
                {
                    // Check if the category already exists
                    var existingCategory = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Name == topic.Category);

                    if (existingCategory == null)
                    {
                        // Add new category
                        var newCategory = new WiseUpDude.Data.Entities.Category
                        {
                            Name = topic.Category,
                            Description = topic.CategoryDescription
                        };

                        _context.Categories.Add(newCategory);
                        await _context.SaveChangesAsync();

                        categoryMap[topic.Category] = newCategory;
                    }
                    else
                    {
                        categoryMap[topic.Category] = existingCategory;
                    }
                }
            }

            // Fix for CS9035: Ensure the required 'Category' property is set in the object initializer
            var entityTopics = modelTopics.Select(mt => new WiseUpDude.Data.Entities.Topic
            {
                Name = mt.Name,
                Description = mt.Description,
                CategoryId = categoryMap.ContainsKey(mt.Category) ? categoryMap[mt.Category].Id : 0, // assign based on CategoryName lookup
                TopicCreationRunId = topicCreationRun.Id,
                TopicCreationRun = new WiseUpDude.Data.Entities.TopicCreationRun
                {
                    Id = topicCreationRun.Id,
                    CreationDate = topicCreationRun.CreationDate,
                    Llm = topicCreationRun.Llm
                },
                Category = categoryMap.ContainsKey(mt.Category) ? categoryMap[mt.Category] : null // Reuse existing category
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
                    Description = t.Description,
                    CategoryId = t.CategoryId,                      // new mapping field
                    Category = t.Category.Name,                 // mapped from the Category navigation property
                    CategoryDescription = t.Category.Description    // mapped from Category.Description
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
                    Description = t.Description,
                    CategoryId = t.CategoryId,
                    Category = t.Category.Name,
                    CategoryDescription = t.Category.Description
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
                // Fix for CS9035: Ensure the required 'Category' property is set in the object initializer
                entity.Topics = model.Topics.Select(t => new Entities.Topic
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    CategoryId = t.CategoryId,  // use the model's CategoryId
                    TopicCreationRunId = entity.Id,
                    TopicCreationRun = entity, // Set the required TopicCreationRun property
                    Category = new Entities.Category // Ensure the required 'Category' property is set
                    {
                        Id = t.CategoryId, // Use the CategoryId from the model
                        Name = t.Category ?? "Default Category", // Provide a default value if t.Category is null
                        Description = t.CategoryDescription ?? "Default Description" // Provide a default value if t.CategoryDescription is null
                    }
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
