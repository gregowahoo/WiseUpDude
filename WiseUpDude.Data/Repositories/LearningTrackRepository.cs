using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;
using Model = WiseUpDude.Model;

public class LearningTrackRepository : ILearningTrackRepository
{
    private readonly ApplicationDbContext _context;
    public LearningTrackRepository(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<Model.LearningTrack>> GetAllAsync()
    {
        var entities = await _context.LearningTracks.Include(x => x.Categories).ToListAsync();
        return entities.Select(EntityToModel);
    }

    public async Task<Model.LearningTrack?> GetByIdAsync(int id)
    {
        var entity = await _context.LearningTracks.Include(x => x.Categories).FirstOrDefaultAsync(x => x.Id == id);
        return entity == null ? null : EntityToModel(entity);
    }

    public async Task AddAsync(Model.LearningTrack model)
    {
        var entity = ModelToEntity(model);
        _context.LearningTracks.Add(entity);
        await _context.SaveChangesAsync();
        model.Id = entity.Id; // update model with generated ID
    }

    public async Task UpdateAsync(Model.LearningTrack model)
    {
        var entity = await _context.LearningTracks.Include(x => x.Categories).FirstOrDefaultAsync(x => x.Id == model.Id);
        if (entity == null) return;
        entity.Name = model.Name;
        entity.Description = model.Description;
        // update other fields as needed
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.LearningTracks.FindAsync(id);
        if (entity != null)
        {
            _context.LearningTracks.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    // --- Mapping helpers ---
    private static Model.LearningTrack EntityToModel(LearningTrack entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        UserId = entity.UserId,
        CreationDate = entity.CreationDate,
        Categories = entity.Categories?.Select(EntityToModel).ToList() ?? new()
    };

    private static Model.LearningTrackCategory EntityToModel(LearningTrackCategory entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        Difficulty = entity.Difficulty,
        LearningTrackId = entity.LearningTrackId,
        CreationDate = entity.CreationDate,
        Sources = entity.Sources?.Select(EntityToModel).ToList() ?? new()
    };

    private static Model.LearningTrackSource EntityToModel(LearningTrackSource entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        SourceType = entity.SourceType,
        Url = entity.Url,
        Description = entity.Description,
        LearningTrackCategoryId = entity.LearningTrackCategoryId,
        CreationDate = entity.CreationDate
        // Quizzes mapping if needed
    };

    private static LearningTrack ModelToEntity(Model.LearningTrack model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        Description = model.Description,
        UserId = model.UserId,
        Categories = model.Categories?.Select(ModelToEntity).ToList() ?? new()
    };

    private static LearningTrackCategory ModelToEntity(Model.LearningTrackCategory model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        Description = model.Description,
        Difficulty = model.Difficulty,
        LearningTrackId = model.LearningTrackId,
        Sources = model.Sources?.Select(ModelToEntity).ToList() ?? new()
    };

    private static LearningTrackSource ModelToEntity(Model.LearningTrackSource model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        SourceType = model.SourceType,
        Url = model.Url,
        Description = model.Description,
        LearningTrackCategoryId = model.LearningTrackCategoryId
        // Quizzes mapping if needed
    };
}