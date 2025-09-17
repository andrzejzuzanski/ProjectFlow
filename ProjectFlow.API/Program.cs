
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjectFlow.API.Endpoints;
using ProjectFlow.API.Hubs;
using ProjectFlow.API.Middleware;
//using ProjectFlow.API.Services;
using ProjectFlow.Core.Configuration;
using ProjectFlow.Core.Interfaces;
using ProjectFlow.Core.Mappings;
using ProjectFlow.Core.Validators;
using ProjectFlow.Infrastructure.Data;
using ProjectFlow.Infrastructure.Repositories;
using ProjectFlow.Infrastructure.Services;
using Serilog;
using System.Text;


namespace ProjectFlow.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                    .Build())
                .CreateLogger();

            try
            {
                Log.Information("Starting ProjectFlow API");

                var builder = WebApplication.CreateBuilder(args);
                builder.Host.UseSerilog();

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

                // Add SignalR
                builder.Services.AddSignalR();

                // Add CORS
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("ReactApp", policy =>
                    {
                        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
                });

                // Configure JWT settings
                builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

                // Add services
                builder.Services.AddScoped<IJwtService, ProjectFlow.Infrastructure.Services.JwtService>();
                builder.Services.AddScoped<INotificationService, ProjectFlow.API.Services.NotificationService>();

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

                Console.WriteLine("DEBUG: About to map hubs");

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseCors("ReactApp");

                // Enable serving static files
                app.UseStaticFiles();

                app.UseHttpsRedirection();
                app.UseAuthentication();
                app.UseAuthorization();

                // Request logging middleware AFTER authentication
                app.UseMiddleware<RequestLoggingMiddleware>();

                // Map SignalR hubs
                app.MapHub<TaskUpdatesHub>("/hubs/task-updates");
                app.MapHub<SimpleHub>("/hubs/simple");
                Console.WriteLine("DEBUG: Both hubs mapped");

                // Map endpoints
                app.MapAuthEndpoints();
                app.MapUserEndpoints();
                app.MapProjectEndpoints();
                app.MapTaskEndpoints();

                Console.WriteLine("DEBUG: All endpoints mapped, starting app");

                app.Run();

            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
