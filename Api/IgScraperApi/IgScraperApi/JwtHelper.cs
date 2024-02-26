using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IgScraperApi
{
    public class JwtHelper
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtHelper(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _config = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GenToken(string ig_id)
        {
            //JWT 簽章使⽤的對稱式加密的⾦鑰
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["TokenSettings:IssuerSigningKey"]));

            // HmacSha256 SecurityAlgorithms
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var claims = new List<Claim>()
            {
                new Claim("Id", ig_id),
                new Claim(ClaimTypes.Role, "ScaperUser")
            };

            // 建立 SecurityTokenDescriptor (JWT PayLoad)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _config["TokenSettings:Issuer"],
                //Audience = _config["TokenSettings:Audience"],
                NotBefore = DateTime.Now,
                IssuedAt = DateTime.Now,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(int.Parse(_config["TokenSettings:ExpireMin"])),
                SigningCredentials = signingCredentials
            };

            // JWT securityToken
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var serializeToken = tokenHandler.WriteToken(securityToken);

            return serializeToken;
        }

        /// <summary>
        /// 從Token中取得Id
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public string GetIdFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // Read the JWT token and parse it into a SecurityToken
            var parsedToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            if (parsedToken != null)
            {
                // Retrieve the id from the JWT payload (assuming the payload contains a claim named "Id")
                if (parsedToken.Payload.TryGetValue("Id", out var id))
                {
                    // Convert the Id to a string and return
                    if (id is string)
                    {
                        return (string)id;
                    }
                }
            }

            // If parsing or retrievingid fails, return a default value or throw an exception based on your scenario
            return null; // Or throw an exception to indicate parsing failure
        }
    }
}
