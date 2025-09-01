using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Model;
using WiseUpDude.Services;
using WiseUpDude.Shared.Services;
using Microsoft.Extensions.Logging;
using WiseUpDude.Shared.Services.Interfaces;
using System;

namespace WiseUpDude.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecialQuizAssignmentsController : ControllerBase
    {
        private readonly SpecialQuizAssignmentService _service;
        private readonly IAssignmentTypeService _typeService;
        private readonly ILogger<SpecialQuizAssignmentsController> _logger;
        public SpecialQuizAssignmentsController(SpecialQuizAssignmentService service, IAssignmentTypeService typeService, ILogger<SpecialQuizAssignmentsController> logger)
        {
            _service = service;
            _typeService = typeService;
            _logger = logger;
        }
        [HttpGet]
        public async Task<ActionResult<List<SpecialQuizAssignment>>> GetAll()
        {
            try
            {
                _logger.LogInformation("Getting all SpecialQuizAssignments");
                return await _service.GetAllAsync();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetAll");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<List<SpecialQuizAssignment>>> GetActive([FromQuery] DateTime? asOf = null)
        {
            try
            {
                var asOfUtc = (asOf ?? DateTime.UtcNow).ToUniversalTime();
                _logger.LogInformation("Getting active SpecialQuizAssignments as of {AsOf}", asOfUtc);
                return await _service.GetActiveAsync(asOfUtc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetActive");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("type/{typeId}")]
        public async Task<ActionResult<List<SpecialQuizAssignment>>> GetByType(int typeId)
        {
            try
            {
                _logger.LogInformation("Getting SpecialQuizAssignments by typeId={TypeId}", typeId);
                return await _service.GetByTypeAsync(typeId);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetByType");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("types")]
        public async Task<ActionResult<List<AssignmentType>>> GetTypes()
        {
            try
            {
                _logger.LogInformation("Getting all AssignmentTypes");
                return await _typeService.GetAllAsync();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetTypes");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPost]
        public async Task<ActionResult> Add(SpecialQuizAssignment assignment)
        {
            _logger.LogInformation("Entered Add method in SpecialQuizAssignmentsController. Payload: {@Assignment}", assignment);
            try
            {
                _logger.LogInformation("Adding SpecialQuizAssignment: {@Assignment}", assignment);
                await _service.AddAsync(assignment);
                _logger.LogInformation("Successfully added SpecialQuizAssignment.");
                return Ok();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in Add. Payload: {@Assignment}", assignment);
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPut]
        public async Task<ActionResult> Update(SpecialQuizAssignment assignment)
        {
            try
            {
                _logger.LogInformation("Updating SpecialQuizAssignment: {@Assignment}", assignment);
                await _service.UpdateAsync(assignment);
                return Ok();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in Update. Payload: {@Assignment}", assignment);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/deactivate")]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var assignment = await _service.GetByIdAsync(id);
                if (assignment == null)
                {
                    return NotFound();
                }
                assignment.EndDate = DateTime.UtcNow;
                await _service.UpdateAsync(assignment);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Deactivate. Id={Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Deleting SpecialQuizAssignment with Id={Id}", id);
                await _service.DeleteAsync(id);
                return Ok();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in Delete. Id={Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
