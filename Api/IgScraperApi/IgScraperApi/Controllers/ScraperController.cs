using CoreLib.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IgScraperApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ScraperController : ControllerBase
    {
        private IIOService _iOService;
        private JwtHelper _jwt;

        public ScraperController(IIOService iOService, JwtHelper jwt)
        {
            _iOService = iOService;
            _jwt = jwt;
        }

        [HttpGet]
        public async Task<IActionResult> GetFollowing()
        {
            await Task.Run(() => { });
            string bearer = HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(bearer))
            {
                var id = _jwt.GetIdFromToken(bearer.Substring(7));
                if (id == null) return BadRequest(new { Message = "Cannot get Id from token" });
                var list = _iOService.ExtractJsonStrings("IgData", id, "Following");
                return Ok(list);
            }
            
            return BadRequest(new { Message = "Invalid token" });
        }

        [HttpGet]
        public async Task<IActionResult> GetFollower()
        {
            await Task.Run(() => { });
            string bearer = HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(bearer))
            {
                var id = _jwt.GetIdFromToken(bearer.Substring(7));
                if (id == null) return BadRequest(new { Message = "Cannot get Id from token" });
                var list = _iOService.ExtractJsonStrings("IgData", id, "Follower");
                return Ok(list);
            }

            return BadRequest(new { Message = "Invalid token" });
        }
    }
}
