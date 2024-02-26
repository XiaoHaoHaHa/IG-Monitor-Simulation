using CoreLib.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IgScraperApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BinaryController : ControllerBase
    {
        private IHttpRequestService _httpRequestService;

        public BinaryController(IHttpRequestService httpRequestService)
        {
            _httpRequestService = httpRequestService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPictureBase64(string url)
        {
            (var result, var content) = await _httpRequestService.GetUrlPictureAsync(url);
            if (result)
            {
                return Ok(new { Content = content });
            }

            return BadRequest(new { Message = content });
        }
    }
}
