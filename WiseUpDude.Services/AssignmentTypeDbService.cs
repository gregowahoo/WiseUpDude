using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiseUpDude.Shared.Services;
using WiseUpDude.Model;
using WiseUpDude.Data.Repositories;

namespace WiseUpDude.Services
{
    public class AssignmentTypeDbService : IAssignmentTypeService
    {
        private readonly AssignmentTypeRepository _repo;
        public AssignmentTypeDbService(AssignmentTypeRepository repo)
        {
            _repo = repo;
        }
        public async Task<List<AssignmentType>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }
        public async Task<AssignmentType> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }
    }
}
