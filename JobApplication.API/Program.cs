using System.Reflection;
using JobApplication.API.Authentication;
using JobApplication.API.Services;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Services;
using JobApplication.Infrastructure.Persistence;
using JobApplication.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace JobApplication.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            var connectionString =
            builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string"
                + "'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddScoped<IJobService, JobService>();
            builder.Services.AddScoped<JobService>();
            builder.Services.AddScoped<IJobCandidateApplicationService, JobCandidateApplicationService>();
            builder.Services.AddScoped<ApplicationService>();
            builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
            builder.Services.AddScoped<IJobRepository, JobRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddSingleton<TokenService>();
            builder.Services.AddSingleton<ITokenService>(sp => sp.GetRequiredService<TokenService>());

            builder.Services.AddAuthentication("Bearer")
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, JobApplication.API.Authentication.TokenAuthenticationHandler>("Bearer", null);
            builder.Services.AddAuthorization();

            builder.Services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(JobApplication.Application.AssemblyReference).Assembly));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Job Application API",
                    Version = "v1",
                    Description = "API for managing job postings and candidate applications."
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Job Application API v1");
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
