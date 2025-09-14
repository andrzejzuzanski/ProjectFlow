
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectFlow.API.Endpoints;
using ProjectFlow.Core.Interfaces;
using ProjectFlow.Core.Mappings;
using ProjectFlow.Core.Validators;
using ProjectFlow.Infrastructure.Data;
using ProjectFlow.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ProjectFlow.Core.Configuration;
using ProjectFlow.Infrastructure.Services;
using System.Text;
using Microsoft.OpenApi.Models;


namespace ProjectFlow.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add DbContext with SQL Server provider
            builder.Services.AddDbContext<ProjectFlowDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            //Add repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
            builder.Services.AddScoped<ITaskRepository, TaskRepository>();

            //Add FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();

            // Add AutoMapper
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<UserMappingProfile>();
                cfg.AddProfile<ProjectMappingProfile>();
            });

            // Configure JWT settings
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

            // Add JWT Service
            builder.Services.AddScoped<IJwtService, JwtService>();

            // Configure JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
            var key = Encoding.UTF8.GetBytes(jwtSettings!.SecretKey);

            // Add Authorization Policies
            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("Admin", policy => policy.RequireRole("Admin"))
                .AddPolicy("ProjectManager", policy => policy.RequireRole("Admin", "ProjectManager"))
                .AddPolicy("Developer", policy => policy.RequireRole("Admin", "ProjectManager", "Developer"));

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            // Map endpoints
            app.MapAuthEndpoints();
            app.MapUserEndpoints();
            app.MapProjectEndpoints();
            app.MapTaskEndpoints();

            app.Run();
        }
    }
}
