using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace IgScraperApi.Middleware
{
    public static class JwtAuthExtension
    {
        /// <summary>
        /// 設定驗證JWT擴充方法
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure JWT Token based Authentication         
            // Install Microsoft.AspNetCore.Authentication.JwtBearer package
            services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }
            ).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;

                options.Events = new JwtBearerEvents();

                //授權成功後是否保存 Token
                options.SaveToken = false;

                //JWT檢驗失敗的回應處理
                options.Events.OnChallenge = context =>
                {
                    // 當驗證失敗時，回應標頭包含 WWW-Authenticate 標頭，顯⽰失敗的詳細原因
                    options.IncludeErrorDetails = true;

                    context.HandleResponse();
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    var responseContent = JsonConvert.SerializeObject(new
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = "Invalid token",
                        Data = ""
                    });

                    return context.Response.WriteAsync(responseContent);
                };

                //JWT權限錯誤的回應處理
                options.Events.OnForbidden = context =>
                {
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                    var responseContent = JsonConvert.SerializeObject(new
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = "You are not authorized to access this resource.",
                        Data = ""
                    });

                    return context.Response.WriteAsync(responseContent);
                };

                //設定要檢驗的JWT內容
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidIssuer = configuration["TokenSettings:Issuer"],
                    //ValidAudience = configuration["TokenSettings:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["TokenSettings:IssuerSigningKey"])),
                    //緩衝時間
                    ClockSkew = TimeSpan.FromSeconds(0),
                    RequireExpirationTime = true
                };
            });
        }
    }
}
