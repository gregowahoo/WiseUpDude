using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiseUpDude.Shared.Services;
using WiseUpDude.Data.Entities;
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
        public async Task<List<AssignmentTypeDto>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            return entities.Select(e => new AssignmentTypeDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description
            }).ToList();
        }
        public async Task<AssignmentTypeDto> GetByIdAsync(int id)
        {
            var e = await _repo.GetByIdAsync(id);
            return new AssignmentTypeDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description
            };
        }
    }
}
