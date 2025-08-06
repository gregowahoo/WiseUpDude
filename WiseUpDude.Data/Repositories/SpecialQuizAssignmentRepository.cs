using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class AssignmentTypeRepository
    {
        private readonly ApplicationDbContext _context;
        public AssignmentTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<WiseUpDude.Model.AssignmentType>> GetAllAsync()
        {
            var entities = await _context.AssignmentTypes.ToListAsync();
            return entities.Select(ToModel).ToList();
        }
        public async Task<WiseUpDude.Model.AssignmentType> GetByIdAsync(int id)
        {
            var entity = await _context.AssignmentTypes.FindAsync(id);
            return entity == null ? null : ToModel(entity);
        }
        private static WiseUpDude.Model.AssignmentType ToModel(Data.Entities.AssignmentType entity)
        {
            return new WiseUpDude.Model.AssignmentType
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description
            };
        }
    }

    public class SpecialQuizAssignmentRepository
    {
        private readonly ApplicationDbContext _context;
        public SpecialQuizAssignmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<WiseUpDude.Model.SpecialQuizAssignment>> GetAllAsync()
        {
            var entities = await _context.SpecialQuizAssignments.ToListAsync();
            return entities.Select(ToModel).ToList();
        }
        public async Task<WiseUpDude.Model.SpecialQuizAssignment> GetByIdAsync(int id)
        {
            var entity = await _context.SpecialQuizAssignments.FindAsync(id);
            return entity == null ? null : ToModel(entity);
        }
        public async Task<List<WiseUpDude.Model.SpecialQuizAssignment>> GetByTypeAsync(int assignmentTypeId)
        {
            var entities = await _context.SpecialQuizAssignments
                .Where(a => a.AssignmentTypeId == assignmentTypeId)
                .ToListAsync();
            return entities.Select(ToModel).ToList();
        }
        public async Task AddAsync(WiseUpDude.Model.SpecialQuizAssignment model)
        {
            var entity = ToEntity(model);
            _context.SpecialQuizAssignments.Add(entity);
            await _context.SaveChangesAsync();
            model.Id = entity.Id;
        }
        public async Task UpdateAsync(WiseUpDude.Model.SpecialQuizAssignment model)
        {
            var entity = ToEntity(model);
            _context.SpecialQuizAssignments.Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var entity = await _context.SpecialQuizAssignments.FindAsync(id);
            if (entity != null)
            {
                _context.SpecialQuizAssignments.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        private static WiseUpDude.Model.SpecialQuizAssignment ToModel(Data.Entities.SpecialQuizAssignment entity)
        {
            return new WiseUpDude.Model.SpecialQuizAssignment
            {
                Id = entity.Id,
                UserQuizId = entity.UserQuizId,
                AssignedByUserId = entity.AssignedByUserId,
                AssignmentTypeId = entity.AssignmentTypeId,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Notes = entity.Notes,
                CreatedAt = entity.CreatedAt
            };
        }
        private static Data.Entities.SpecialQuizAssignment ToEntity(WiseUpDude.Model.SpecialQuizAssignment model)
        {
            return new Data.Entities.SpecialQuizAssignment
            {
                Id = model.Id,
                UserQuizId = model.UserQuizId,
                AssignedByUserId = model.AssignedByUserId,
                AssignmentTypeId = model.AssignmentTypeId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Notes = model.Notes,
                CreatedAt = model.CreatedAt
            };
        }
    }
}
