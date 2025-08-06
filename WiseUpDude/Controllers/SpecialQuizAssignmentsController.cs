using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Data.Entities;
using WiseUpDude.Services;
using WiseUpDude.Shared.Services;

namespace WiseUpDude.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecialQuizAssignmentsController : ControllerBase
    {
        private readonly SpecialQuizAssignmentService _service;
        private readonly IAssignmentTypeService _typeService;
        public SpecialQuizAssignmentsController(SpecialQuizAssignmentService service, IAssignmentTypeService typeService)
        {
            _service = service;
            _typeService = typeService;
        }
        [HttpGet]
        public async Task<ActionResult<List<SpecialQuizAssignment>>> GetAll()
        {
            return await _service.GetAllAsync();
        }
        [HttpGet("type/{typeId}")]
        public async Task<ActionResult<List<SpecialQuizAssignment>>> GetByType(int typeId)
        {
            return await _service.GetByTypeAsync(typeId);
        }
        [HttpGet("types")]
        public async Task<ActionResult<List<AssignmentTypeDto>>> GetTypes()
        {
            return await _typeService.GetAllAsync();
        }
        [HttpPost]
        public async Task<ActionResult> Add(SpecialQuizAssignment assignment)
        {
            await _service.AddAsync(assignment);
            return Ok();
        }
        [HttpPut]
        public async Task<ActionResult> Update(SpecialQuizAssignment assignment)
        {
            await _service.UpdateAsync(assignment);
            return Ok();
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok();
        }
    }
}
