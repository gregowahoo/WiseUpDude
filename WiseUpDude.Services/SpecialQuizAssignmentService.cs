using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Model;
using WiseUpDude.Data.Repositories;
using System; // added for DateTime

namespace WiseUpDude.Services
{

    public class SpecialQuizAssignmentService
    {
        private readonly SpecialQuizAssignmentRepository _repo;
        public SpecialQuizAssignmentService(SpecialQuizAssignmentRepository repo)
        {
            _repo = repo;
        }
        public Task<List<SpecialQuizAssignment>> GetAllAsync() => _repo.GetAllAsync();
        public Task<SpecialQuizAssignment> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task<List<SpecialQuizAssignment>> GetByTypeAsync(int assignmentTypeId) => _repo.GetByTypeAsync(assignmentTypeId);
        public Task<List<SpecialQuizAssignment>> GetActiveAsync(DateTime asOfUtc) => _repo.GetActiveAsync(asOfUtc);
        public Task AddAsync(SpecialQuizAssignment assignment) => _repo.AddAsync(assignment);
        public Task UpdateAsync(SpecialQuizAssignment assignment) => _repo.UpdateAsync(assignment);
        public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
    }
}
