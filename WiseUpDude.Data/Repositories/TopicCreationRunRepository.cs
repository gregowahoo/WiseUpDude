using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class TopicCreationRunRepository : ITopicCreationRunRepository<WiseUpDude.Model.TopicCreationRun>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public TopicCreationRunRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task AddAsync(WiseUpDude.Model.TopicCreationRun entity)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
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

            await context.TopicCreationRuns.AddAsync(entityTopicCreationRun);
            await context.SaveChangesAsync();

            entity.Id = entityTopicCreationRun.Id;
        }

        public async Task AddAsync(WiseUpDude.Model.TopicCreationRun topicCreationRun, List<WiseUpDude.Model.Topic> modelTopics)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var categoryMap = new Dictionary<string, WiseUpDude.Data.Entities.Category>();
            foreach (var topic in modelTopics)
            {
                if (!string.IsNullOrWhiteSpace(topic.Category))
                {
                    var existingCategory = await context.Categories
                        .FirstOrDefaultAsync(c => c.Name == topic.Category);

                    if (existingCategory == null)
                    {
                        var newCategory = new WiseUpDude.Data.Entities.Category
                        {
                            Name = topic.Category,
                            Description = topic.CategoryDescription
                        };

                        context.Categories.Add(newCategory);
                        await context.SaveChangesAsync();

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

            await context.TopicCreationRuns.AddAsync(entityTopicCreationRun);
            await context.SaveChangesAsync();

            topicCreationRun.Id = entityTopicCreationRun.Id;
        }

        public async Task<WiseUpDude.Model.TopicCreationRun> GetByIdAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.TopicCreationRuns
                .Include(t => t.Topics)
                .ThenInclude(t => t.Category) // Ensure Category is included
                .FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null)
                throw new KeyNotFoundException($"TopicCreationRun with Id {id} not found.");

            return new WiseUpDude.Model.TopicCreationRun
            {
                Id = entity.Id,
                CreationDate = entity.CreationDate,
                Llm = entity.Llm,
                Topics = entity.Topics?.Select(t => new WiseUpDude.Model.Topic
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

        public async Task<IEnumerable<WiseUpDude.Model.TopicCreationRun>> GetAllAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entities = await context.TopicCreationRuns
                .Include(t => t.Topics)
                .ThenInclude(t => t.Category) // Ensure Category is included
                .ToListAsync();

            return entities.Select(entity => new WiseUpDude.Model.TopicCreationRun
            {
                Id = entity.Id,
                CreationDate = entity.CreationDate,
                Llm = entity.Llm,
                Topics = entity.Topics?.Select(t => new WiseUpDude.Model.Topic
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

        public async Task UpdateAsync(WiseUpDude.Model.TopicCreationRun model)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.TopicCreationRuns
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

            context.TopicCreationRuns.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.TopicCreationRuns.FindAsync(id);
            if (entity != null)
            {
                context.TopicCreationRuns.Remove(entity);
                await context.SaveChangesAsync();
            }
        }
    }
}
