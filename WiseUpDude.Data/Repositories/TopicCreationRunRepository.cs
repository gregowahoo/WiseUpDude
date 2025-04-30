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
            var entityTopicCreationRun = new WiseUpDude.Data.Entities.TopicCreationRun
            {
                CreationDate = entity.CreationDate,
                Llm = entity.Llm,
                Topics = entity.Topics?.Select(t => new WiseUpDude.Data.Entities.Topic
                {
                    Name = t.Name,
                    Description = t.Description,
                    CategoryId = t.CategoryId, // Removed '?? 0' since CategoryId is non-nullable
                    TopicCreationRunId = entity.Id,
                    TopicCreationRun = new WiseUpDude.Data.Entities.TopicCreationRun
                    {
                        Id = entity.Id,
                        CreationDate = entity.CreationDate,
                        Llm = entity.Llm
                    },
                    Category = t.Category != null ? new WiseUpDude.Data.Entities.Category
                    {
                        Id = t.CategoryId,
                        Name = t.Category ?? "Default Category",
                        Description = t.CategoryDescription ?? "Default Description"
                    } : null
                }).ToList() ?? new List<WiseUpDude.Data.Entities.Topic>()
            };

            await _context.TopicCreationRuns.AddAsync(entityTopicCreationRun);
            await _context.SaveChangesAsync();

            entity.Id = entityTopicCreationRun.Id;
        }

        public async Task AddAsync(Model.TopicCreationRun topicCreationRun, List<Model.Topic> modelTopics)
        {
            var categoryMap = new Dictionary<string, WiseUpDude.Data.Entities.Category>();
            foreach (var topic in modelTopics)
            {
                if (!string.IsNullOrWhiteSpace(topic.Category))
                {
                    var existingCategory = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Name == topic.Category);

                    if (existingCategory == null)
                    {
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

            var entityTopics = modelTopics.Select(mt => new WiseUpDude.Data.Entities.Topic
            {
                Name = mt.Name,
                Description = mt.Description,
                CategoryId = categoryMap.ContainsKey(mt.Category) ? categoryMap[mt.Category].Id : 0,
                TopicCreationRunId = topicCreationRun.Id,
                TopicCreationRun = new WiseUpDude.Data.Entities.TopicCreationRun
                {
                    Id = topicCreationRun.Id,
                    CreationDate = topicCreationRun.CreationDate,
                    Llm = topicCreationRun.Llm
                },
                Category = categoryMap.ContainsKey(mt.Category) ? categoryMap[mt.Category] : null
            }).ToList();

            var entityTopicCreationRun = new WiseUpDude.Data.Entities.TopicCreationRun
            {
                Id = topicCreationRun.Id,
                CreationDate = topicCreationRun.CreationDate,
                Llm = topicCreationRun.Llm,
                Topics = entityTopics
            };

            await _context.TopicCreationRuns.AddAsync(entityTopicCreationRun);
            await _context.SaveChangesAsync();

            topicCreationRun.Id = entityTopicCreationRun.Id;
        }

        public async Task<Model.TopicCreationRun> GetByIdAsync(int id)
        {
            var entity = await _context.TopicCreationRuns
                .Include(t => t.Topics)
                .ThenInclude(t => t.Category) // Ensure Category is included
                .FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null)
                throw new KeyNotFoundException($"TopicCreationRun with Id {id} not found.");

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
                    CategoryId = t.CategoryId ?? 0, // Use 0 as a default value for CategoryId
                    Category = t.Category?.Name ?? "Uncategorized", // Handle null Category
                    CategoryDescription = t.Category?.Description ?? string.Empty // Handle null Category.Description
                }).ToList()
            };
        }

        public async Task<IEnumerable<Model.TopicCreationRun>> GetAllAsync()
        {
            var entities = await _context.TopicCreationRuns
                .Include(t => t.Topics)
                .ThenInclude(t => t.Category) // Ensure Category is included
                .ToListAsync();

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
                    CategoryId = t.CategoryId ?? 0, // Use 0 as a default value for CategoryId
                    Category = t.Category?.Name ?? "Uncategorized", // Handle null Category
                    CategoryDescription = t.Category?.Description ?? string.Empty // Handle null Category.Description
                }).ToList()
            });
        }

        public async Task UpdateAsync(Model.TopicCreationRun model)
        {
            var entity = await _context.TopicCreationRuns
                .Include(t => t.Topics)
                .FirstOrDefaultAsync(t => t.Id == model.Id);

            if (entity == null)
                throw new KeyNotFoundException($"TopicCreationRun with Id {model.Id} not found.");

            entity.Llm = model.Llm;
            entity.CreationDate = model.CreationDate;

            if (model.Topics != null)
            {
                entity.Topics = model.Topics.Select(t => new Entities.Topic
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    CategoryId = t.CategoryId,
                    TopicCreationRunId = entity.Id,
                    TopicCreationRun = entity,
                    Category = t.Category != null ? new Entities.Category
                    {
                        Id = t.CategoryId,
                        Name = t.Category ?? "Default Category",
                        Description = t.CategoryDescription ?? "Default Description"
                    } : null
                }).ToList();
            }

            _context.TopicCreationRuns.Update(entity);
            await _context.SaveChangesAsync();
        }

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
