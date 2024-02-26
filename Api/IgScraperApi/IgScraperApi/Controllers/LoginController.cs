using CoreLib;
using CoreLib.DataModels;
using CoreLib.Interfaces;
using IgScraperApi.RequestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IgScraperApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private ILoginService _loginService;
        private JwtHelper _jwt;       

        public LoginController(ILoginService loginService, JwtHelper jwt)
        {
            _loginService = loginService;
            _jwt = jwt;       
        }

        [HttpPost]
        public async Task<IActionResult> GetUserData(LoginModel loginModel)
        {
            (bool result, string msg, IgScraper IgScraper) = await _loginService.InitializeDataAsync(loginModel.Sessionid);
            if (result)
            {
                var token = _jwt.GenToken(IgScraper.Ig_id);
                return Ok(new { Token = token, Message = msg, UserInfo = IgScraper });
            }

            return BadRequest(new { Message = msg });
        }

        [Authorize]
        public async Task<IActionResult> ChangeToken()
        {
            await Task.Run(() => { }); //為了讓ChangeToken非同步呼叫

            string bearer = HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(bearer))
            {
                var id = _jwt.GetIdFromToken(bearer.Substring(7));
                if (id == null) return BadRequest(new { Message = "Cannot get Id from token" });
                var isSuccess = IgScraper.IgScrapers.TryGetValue(id, out var igScraper);
                if (!isSuccess) return BadRequest(new { Message = "Cannot get Id from Scrapers" });
                var newToken = _jwt.GenToken(id);
                return Ok(new { Token = newToken });
            }
            return BadRequest(new { Message = "Invalid Token" });
        }
    }
}
