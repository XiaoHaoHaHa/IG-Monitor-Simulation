using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace IgScraperApi.Middleware
{
    public static class JsonValidationExtension
    {
        public static IActionResult ValidationErrorHandler(ApiBehaviorOptions apiBehaviorOptions, ActionContext actionContext)
        {
            var originalFactory = apiBehaviorOptions.InvalidModelStateResponseFactory;
            if (actionContext.ModelState.IsValid)
            {
                return originalFactory(actionContext);
            }

            var traceId = Activity.Current?.Id ?? actionContext.HttpContext.TraceIdentifier;

            var errors = actionContext.ModelState.ToDictionary(
                    p => p.Key,
                    p => p.Value.Errors.Select(e => e.ErrorMessage).ToList()
                );

            actionContext.HttpContext.Response.StatusCode = 400;

            //複寫錯誤內容
            return new BadRequestObjectResult(new
            {
                StatusCode = actionContext.HttpContext.Response.StatusCode,
                Message = $"輸入格式錯誤: {errors.First().Value[0]}",
                Data = errors
            });
        }
    }
}
