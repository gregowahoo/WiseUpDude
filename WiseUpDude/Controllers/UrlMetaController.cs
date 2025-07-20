using Microsoft.AspNetCore.Mvc;
using WiseUpDude.Services;
using WiseUpDude.Shared.Model;
using System.Threading.Tasks;

namespace WiseUpDude.Controllers
{
    [ApiController]
    [Route("api/urlmeta")]
    public class UrlMetaController : ControllerBase
    {
        private readonly WiseUpDude.Services.UrlMetaService _urlMetaService;

        public UrlMetaController(WiseUpDude.Services.UrlMetaService urlMetaService)
        {
            _urlMetaService = urlMetaService;
        }

        [HttpGet]
        public async Task<ActionResult<UrlMetaResult>> Get([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest();
            var result = await _urlMetaService.GetUrlMetaAsync(url);
            return Ok(result);
        }
    }
}
