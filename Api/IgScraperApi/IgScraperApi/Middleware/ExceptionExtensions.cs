using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using System.Net;

namespace IgScraperApi.Middleware
{
    /// <summary>
    /// 集中全局例外處理擴充方法
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// 全局例外處理方法
        /// </summary>
        /// <param name="app"></param>
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        var msg = new
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = contextFeature.Error.Message,
                            Data = contextFeature.Error.Data
                        };
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(msg));
                    }
                }
                );
            });
        }
    }
}
