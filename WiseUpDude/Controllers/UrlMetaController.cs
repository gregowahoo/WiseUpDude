using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WiseUpDude.Services;
using WiseUpDude.Shared.Model;
using System;
using System.Threading.Tasks;

namespace WiseUpDude.Controllers
{
    [ApiController]
    [Route("api/urlmeta")]
    public class UrlMetaController : ControllerBase
    {
        private readonly WiseUpDude.Services.UrlMetaService _urlMetaService;
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        public UrlMetaController(WiseUpDude.Services.UrlMetaService urlMetaService, IMemoryCache cache)
        {
            _urlMetaService = urlMetaService;
            _cache = cache;
        }

        [HttpGet]
        public async Task<ActionResult<UrlMetaResult>> Get([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest();

            if (_cache.TryGetValue(url, out UrlMetaResult? cachedResult) && cachedResult != null)
            {
                return Ok(cachedResult);
            }

            var result = await _urlMetaService.GetUrlMetaAsync(url);
            _cache.Set(url, result, CacheDuration);
            return Ok(result);
        }
    }
}
