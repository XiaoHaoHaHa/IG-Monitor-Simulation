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

            //DI�`�J
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<JwtHelper>();
            builder.Services.AddScoped<ILoginService, LoginService>();
            builder.Services.AddScoped<IHttpRequestService, HttpRequestService>();
            builder.Services.AddScoped<IFetchFollowSevice, FetchFollowSevice>();
            builder.Services.AddScoped<IIOService, IOService>();
            builder.Services.AddHostedService<InitializationService>();

            //�]�wJWT Bearer Token
            builder.Services.AddJwtAuthentication(builder.Configuration);

            //�ҥ�WebSocket SignalR
            builder.Services.AddSignalR();

            //�]�wCORS�W�h
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

            //�]�wCronJob
            builder.Services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                //�إ� job
                var jobKey = new JobKey("UpdateFollowJob");
                q.AddJob<UpdateFollowJob>(jobKey);
                //�إ� trigger(�W�h) ��Ĳ�o job
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
            //Swagger�]�w
            builder.Services.AddOpenApiDocument(config =>
            {
                config.GenerateEnumMappingDescription = true; //redoc �����P��API�������ͤU�Կ��
                config.Version = "v1"; //�]�w API ������T
                config.Title = "IgScraperApi"; //�]�w���D
                config.DocumentName = "v1"; //�]�wAPI���W��
                config.AddSecurity("Bearer", new NSwag.OpenApiSecurityScheme
                {
                    Description = "JWT�e�����ݭn�B�~�[Bearer",
                    Scheme = JwtBearerDefaults.AuthenticationScheme.ToLower(),
                    Type = NSwag.OpenApiSecuritySchemeType.Http,
                    BearerFormat = "JWT",
                });//�o�q�|�۰ʲ���"Bearer "�r�˩ҥHJWT�e�����ݭn�B�~�[

                config.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Bearer"));
            });

            //��g��e�ݶǰe���ŦX�榡Json�ɦ^�Ǧ۩w�q�����~Response Json
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext => JsonValidationExtension.ValidationErrorHandler(options, actionContext);
            });

            var app = builder.Build();

            // �ҥ�Swagger Configure the HTTP request pipeline.
            app.UseOpenApi();
            if (app.Environment.IsDevelopment()) //��o��{������������SwaggerUI�H���C�t��
                app.UseSwaggerUi3();
            app.UseReDoc(config => // serve ReDoc UI
            {
                // �]�w ReDoc UI ������(�`�N�n�[ / �׽u�}�Y)
                config.Path = "/redoc";
            });

            //�ҥ�CORS(�����b���Ҥ��e)
            app.UseCors();

            //�����ҥ~�B�z�d�I
            app.ConfigureExceptionHandler();

            app.UseHttpsRedirection();

            //���ҥ����b���v���e
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            
            //�}��WebSocket���I(���I�b�̫�}��)
            app.MapHub<BroadcastHub>(nameof(BroadcastHub));
            app.Run();
        }
    }
}