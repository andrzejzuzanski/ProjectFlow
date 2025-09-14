
using Microsoft.EntityFrameworkCore;
using ProjectFlow.Infrastructure.Data;
using ProjectFlow.API.Endpoints;
using ProjectFlow.Core.Interfaces;
using ProjectFlow.Infrastructure.Repositories;
using FluentValidation;
using ProjectFlow.Core.Validators;

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

            //Add FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();

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

            app.Run();
        }
    }
}
