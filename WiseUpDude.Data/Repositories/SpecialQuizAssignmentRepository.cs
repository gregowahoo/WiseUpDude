using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;

namespace WiseUpDude.Data.Repositories
{
    public class AssignmentTypeRepository
    {
        private readonly ApplicationDbContext _context;
        public AssignmentTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<AssignmentType>> GetAllAsync()
        {
            return await _context.AssignmentTypes.ToListAsync();
        }
        public async Task<AssignmentType> GetByIdAsync(int id)
        {
            return await _context.AssignmentTypes.FindAsync(id);
        }
    }

    public class SpecialQuizAssignmentRepository
    {
        private readonly ApplicationDbContext _context;
        public SpecialQuizAssignmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<SpecialQuizAssignment>> GetAllAsync()
        {
            return await _context.SpecialQuizAssignments.ToListAsync();
        }
        public async Task<SpecialQuizAssignment> GetByIdAsync(int id)
        {
            return await _context.SpecialQuizAssignments.FindAsync(id);
        }
        public async Task<List<SpecialQuizAssignment>> GetByTypeAsync(int assignmentTypeId)
        {
            return await _context.SpecialQuizAssignments
                .Where(a => a.AssignmentTypeId == assignmentTypeId)
                .ToListAsync();
        }
        public async Task AddAsync(SpecialQuizAssignment assignment)
        {
            _context.SpecialQuizAssignments.Add(assignment);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(SpecialQuizAssignment assignment)
        {
            _context.SpecialQuizAssignments.Update(assignment);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var assignment = await _context.SpecialQuizAssignments.FindAsync(id);
            if (assignment != null)
            {
                _context.SpecialQuizAssignments.Remove(assignment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
