
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectFlow.API.Endpoints;
using ProjectFlow.Core.Interfaces;
using ProjectFlow.Core.Mappings;
using ProjectFlow.Core.Validators;
using ProjectFlow.Infrastructure.Data;
using ProjectFlow.Infrastructure.Repositories;


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

            //Add FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();

            // Add AutoMapper
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<UserMappingProfile>();
                cfg.AddProfile<ProjectMappingProfile>();
            });

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            // Map endpoints
            app.MapUserEndpoints();
            app.MapProjectEndpoints();

            app.Run();
        }
    }
}
