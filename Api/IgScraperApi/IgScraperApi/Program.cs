using CoreLib;
using CoreLib.Interfaces;
using IgScraperApi.CronJobs;
using IgScraperApi.Middleware;
using IgScraperApi.WebSocketServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using NSwag.Generation.Processors.Security;
using Quartz;

namespace IgScraperApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers().AddNewtonsoftJson();

            // Add services to the container.

            //DI注入
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<JwtHelper>();
            builder.Services.AddScoped<ILoginService, LoginService>();
            builder.Services.AddScoped<IHttpRequestService, HttpRequestService>();
            builder.Services.AddScoped<IFetchFollowSevice, FetchFollowSevice>();
            builder.Services.AddScoped<IIOService, IOService>();
            builder.Services.AddHostedService<InitializationService>();

            //設定JWT Bearer Token
            builder.Services.AddJwtAuthentication(builder.Configuration);

            //啟用WebSocket SignalR
            builder.Services.AddSignalR();

            //設定CORS規則
            var corsUrls = builder.Configuration["CorsUrls"].Split(";");
            for (var i = 0; i < corsUrls.Length; i++)
                corsUrls[i] = corsUrls[i].Trim();
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                builder =>
                {
                    builder.WithOrigins(corsUrls)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            //設定CronJob
            builder.Services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                //建立 job
                var jobKey = new JobKey("UpdateFollowJob");
                q.AddJob<UpdateFollowJob>(jobKey);
                //建立 trigger(規則) 來觸發 job
                q.AddTrigger(t => t
                    .WithIdentity("UpdateFollowTrigger")
                    .ForJob(jobKey)
                    .StartNow()
                    .WithCronSchedule("*/30 * * * * ?")
                );
            });
            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            //Swagger設定
            builder.Services.AddOpenApiDocument(config =>
            {
                config.GenerateEnumMappingDescription = true; //redoc 為不同的API版本產生下拉選單
                config.Version = "v1"; //設定 API 版本資訊
                config.Title = "IgScraperApi"; //設定標題
                config.DocumentName = "v1"; //設定API文件名稱
                config.AddSecurity("Bearer", new NSwag.OpenApiSecurityScheme
                {
                    Description = "JWT前面不需要額外加Bearer",
                    Scheme = JwtBearerDefaults.AuthenticationScheme.ToLower(),
                    Type = NSwag.OpenApiSecuritySchemeType.Http,
                    BearerFormat = "JWT",
                });//這段會自動產生"Bearer "字樣所以JWT前面不需要額外加

                config.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Bearer"));
            });

            //改寫當前端傳送不符合格式Json時回傳自定義的錯誤Response Json
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext => JsonValidationExtension.ValidationErrorHandler(options, actionContext);
            });

            var app = builder.Build();

            // 啟用Swagger Configure the HTTP request pipeline.
            app.UseOpenApi();
            if (app.Environment.IsDevelopment()) //當這支程式正式部取消SwaggerUI以降低負載
                app.UseSwaggerUi3();
            app.UseReDoc(config => // serve ReDoc UI
            {
                // 設定 ReDoc UI 的路由(注意要加 / 斜線開頭)
                config.Path = "/redoc";
            });

            //啟用CORS(必須在驗證之前)
            app.UseCors();

            //全局例外處理攔截
            app.ConfigureExceptionHandler();

            app.UseHttpsRedirection();

            //驗證必須在授權之前
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            
            //開啟WebSocket端點(端點在最後開啟)
            app.MapHub<BroadcastHub>(nameof(BroadcastHub));
            app.Run();
        }
    }
}